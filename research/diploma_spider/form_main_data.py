import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

sns.set(style="white", context="talk")

whole_data = pd.DataFrame(columns=['repo_name', 'method_invocations', 'filter'])

metadata = [('main_data_best_match.csv', 'best_match'),
    ('main_data_most_stars.csv', 'most_stars'),
    ('main_data_most_forks.csv', 'most_forks'),
    ('main_data_recently_updated.csv', 'recently_updated')]

for data in metadata:
    new_data = pd.read_csv(data[0], index_col=0)
    new_data['filter'] = data[1]
    whole_data = whole_data.append(new_data, ignore_index=True)

whole_data['filter'] = whole_data['filter'] + whole_data.duplicated(subset='repo_name').apply(str)
print(whole_data)
whole_data.to_csv('main_data.csv')

sns.scatterplot(x=whole_data.index, y='method_invocations', data=whole_data, hue='filter')
plt.show()
plt.clf()
