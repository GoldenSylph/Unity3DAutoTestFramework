import pandas as pd

files = ['main_data_created_asc.csv', 'main_data_created_desc.csv', 'main_data_stars_asc.csv', 'main_data_stars_desc.csv']

whole_data = pd.DataFrame(columns=['repo_name', 'method_invocations'])

for file in files:
    print('For {}'.format(file))

    main_data = pd.read_csv(file, index_col=0)
    whole_data = whole_data.append(main_data, ignore_index=True)

    whole = len(main_data)

    positive = len(main_data[main_data['method_invocations'] > 0])
    negative = len(main_data[main_data['method_invocations'] == 0])

    print('positive: {} - {} / {}'.format(positive / whole, positive, whole))
    print('negative: {} - {} / {}'.format(negative / whole, negative, whole))

whole_data = whole_data.drop_duplicates()

print(whole_data)

positive = len(whole_data[whole_data['method_invocations'] > 0])
negative = len(whole_data[whole_data['method_invocations'] == 0])
whole = len(whole_data)

print('whole positive: {} - {} / {}'.format(positive / whole, positive, whole))
print('whole negative: {} - {} / {}'.format(negative / whole, negative, whole))
