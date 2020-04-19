import requests
import base64
import re
import sys
import threading
import time
import json
import traceback
import pandas as pd

"""
Цель исследования: доказать что значительная часть репозиториев по юнити использует Input класс для взятия ввода
Метод: сбор данных о всех репозиториях по юнити, подсчёту количества вызовов данного класса
"""
def background_calc():

    saved_urls_name = 'saved_urls4.csv'
    main_data_name = 'main_data_created_asc.csv'

    auth_data = (str(sys.argv[1]), str(sys.argv[2]))

    #Initial url: https://api.github.com/search/repositories?q=unity+language:csharp&sort=stars&order=desc&per_page=100
    saved_urls = pd.read_csv(saved_urls_name, index_col=0)
    print(saved_urls)

    print('User:', auth_data)
    current_url = saved_urls[saved_urls['url_name'] == 'page_url']['url'][0]
    repos_response = requests.get(current_url, auth=auth_data)
    print('Links: {}', repos_response.links)
    last_url = repos_response.links['last']['url']
    total_repo_count = 0
    main_data = pd.read_csv(main_data_name, index_col=0)

    def get(url):
        result = requests.get(url, auth=auth_data)
        if result.status_code == 403:
            reset_time = float(result.headers['X-RateLimit-Reset'])
            sleep_time = reset_time - time.time()
            print('Exceeded rate limit. Waiting for: {}'.format(sleep_time))
            time.sleep(sleep_time)
            result = requests.get(url, auth=auth_data)
        return result

    try:
        while True:

            print('Working with url: {}'.format(current_url))
            raw_repo_data = repos_response.json()
            if total_repo_count == 0:
                total_repo_count = raw_repo_data['total_count']
                print('Acquired total repo count: {}'.format(total_repo_count))
            raw_repo_items = raw_repo_data['items']
            repo_names = [repo["full_name"] for repo in raw_repo_items]
            if pd.notna(saved_urls.at[1, 'url']) and saved_urls.at[1, 'url'] in repo_names:
                repo_names = repo_names[repo_names.index(saved_urls.at[1, 'url']):]
            for repo_name in repo_names:
                print('Working with repo: {}'.format(repo_name))
                code_search_url = 'https://api.github.com/search/code?q=Input+in:file+language:csharp+repo:{}'.format(repo_name)
                code_response = get(code_search_url)
                saved_urls.at[1, 'url'] = repo_name
                code_data = code_response.json()
                method_invokations_count = 0

                code_urls = [code_item['url'] for code_item in code_data['items']]
                if pd.notna(saved_urls.at[2, 'url']) and saved_urls.at[2, 'url'] in code_urls:
                    code_urls = code_urls[code_urls.index(saved_urls.at[2, 'url']):]
                for code_url in code_urls:
                    print('Source url: {}'.format(code_url))
                    source_details_response = get(code_url)
                    source_details_data = source_details_response.json()
                    if 'message' in source_details_data:
                        print(source_details_data['message'])
                        continue
                    try:
                        source = base64.b64decode(source_details_data['content'].encode('utf-8')).decode('utf-8')
                    except:
                        print(traceback.format_exc())
                        continue
                    method_invokations_count_in_source = len(re.findall(r'[(\[(\s,=\+\-\*/\?&|]Input\.', source))
                    method_invokations_count += method_invokations_count_in_source
                    saved_urls.at[2, 'url'] = code_url
                    saved_urls.to_csv(saved_urls_name)

                new_data = pd.DataFrame({'repo_name': repo_name, 'method_invocations': method_invokations_count}, index=[0])
                main_data = main_data.append(new_data, ignore_index=True)
                print('Data gathered:\n{}'.format(new_data))
                main_data.to_csv(main_data_name)

            current_url = repos_response.links['next']['url']
            saved_urls.at[0, 'url'] = current_url
            saved_urls.to_csv(saved_urls_name)
            if current_url == last_url:
                print('Pages are over. Breaking loop...')
                break
            repos_response = get(current_url)

    except Exception as err:
        tb = traceback.format_exc()
        print('Type: {}, Message: {}, {}'.format(type(err), str(err), tb))
    finally:
        print('Emergency saving...')
        main_data.to_csv(main_data_name)
        saved_urls.to_csv(saved_urls_name)

def main():
    print('Starting main thread...')
    thread = threading.Thread(target=background_calc)
    thread.setDaemon(True)
    thread.start()
    print('Searching thread started...')
    while thread.isAlive():
        time.sleep(1)
    print('Searching and gathering work is done, master.')

if __name__ == '__main__':
    main()
