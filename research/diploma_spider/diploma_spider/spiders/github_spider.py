# -*- coding: utf-8 -*-
import scrapy
import requests
import json
import base64
import re
import time

import pandas as pd

class GithubSpiderSpider(scrapy.Spider):

    name = 'github-spider'
    main_data = pd.DataFrame(columns=['repo_name', 'method_invocations'])
    file_name = 'main_data_most_forks.csv'
    config = False

    recently_updated_url = 'https://github.com/search?o=desc&p={}&q=unity+language%3AC%23&s=updated&type=Repositories'
    most_stars_url = 'https://github.com/search?o=desc&p={}&q=unity+language%3AC%23&s=stars&type=Repositories'
    most_forks_url = 'https://github.com/search?o=desc&p={}&q=unity+language%3AC%23&s=forks&type=Repositories'
    best_match_url = 'https://github.com/search?p={}&q=language%3Acsharp+unity&type=Repositories'

    def start_requests(self):
        urls = [self.most_forks_url.format(i + 1) for i in range(101)]
        for url in urls:
            yield scrapy.Request(url=url, callback=self.parse)

    def auth_data(self):
        if self.config:
            return (self.config['login'], self.config['password'])
        with open('github_config.json', 'r', encoding='utf-8') as github_file:
            self.config = json.load(github_file)
            return (self.config['login'], self.config['password'])

    def get(self, url):
        result = requests.get(url, auth=self.auth_data())
        if result.status_code == 403:
            header_name = 'X-RateLimit-Reset'
            if header_name in result.headers:
                reset_time = float(result.headers[header_name])
                sleep_time = reset_time - time.time()
                if sleep_time < 0:
                    self.logger.info('Sleep time is negative, adjusting to 1...')
                    sleep_time = 1
                self.logger.info('Exceeded rate limit. Waiting for: {}'.format(sleep_time))
                time.sleep(sleep_time)
                result = requests.get(url, auth=auth_data)
            else:
                return False
                self.logger.info('{}, status: {}'.format(result.headers, result.status_code))
        return result

    def get_json(self, response):
        try:
            return response.json()
        except:
            self.logger.info(traceback.format_exc())
            return False

    def closed(self, reason):
        self.logger.info('Done. Specified reason: {}'.format(str(reason)))

    def parse(self, response):
        repo_names = [r[1:] for r in response.xpath('//div[@class="f4 text-normal"]/a/@href').extract()]
        for repo_name in repo_names:
            self.logger.info('Working with repo: {}'.format(repo_name))
            code_search_url = 'https://api.github.com/search/code?q=Input+in:file+language:csharp+repo:{}'.format(repo_name)
            code_response = self.get(code_search_url)
            if not code_response:
                self.logger.info('something wrong with header, continue...')
                continue
            code_data = self.get_json(code_response)
            if not code_data:
                continue
            method_invokations_count = 0

            code_urls = [code_item['url'] for code_item in code_data['items']]
            for code_url in code_urls:
                self.logger.info('Source url: {}'.format(code_url))
                source_details_response = self.get(code_url)
                if not source_details_response:
                    self.logger.info('something wrong with header in source details response, continue...')
                    continue
                source_details_data = self.get_json(source_details_response)
                if not source_details_data:
                    continue
                if 'message' in source_details_data:
                    self.logger.info(source_details_data['message'])
                    continue
                try:
                    source = base64.b64decode(source_details_data['content'].encode('utf-8')).decode('utf-8')
                except:
                    self.logger.info(traceback.format_exc())
                    continue
                method_invokations_count_in_source = len(re.findall(r'[(\[(\s,=\+\-\*/\?&|]Input\.', source))
                method_invokations_count += method_invokations_count_in_source

            new_data = pd.DataFrame({'repo_name': repo_name, 'method_invocations': method_invokations_count}, index=[0])
            self.main_data = self.main_data.append(new_data, ignore_index=True)
            self.logger.info('Data gathered:\n{}'.format(new_data))
            self.main_data.to_csv(self.file_name)
