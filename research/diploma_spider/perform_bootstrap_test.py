import math as m
import pandas as pd
import multiprocessing as mp
import seaborn as sns
import matplotlib.pyplot as plt
import matplotlib

def stat(x):
    return round(x.mean(), 3)

if __name__ == '__main__':

    whole_data = pd.read_csv('main_data.csv')

    R = 99509
    n = len(whole_data)
    confidence = 90
    specific_data = whole_data['method_invocations']

    process_count = mp.cpu_count()
    chunk_size = 1000
    print('Processes: {}, chunksize: {}'.format(process_count, chunk_size))

    with mp.Pool(process_count) as pool:
        boot_data = pd.Series(pool.map(stat, [specific_data.sample(n, replace=True) for i in range(R)], chunksize=chunk_size))

    boot_data_len = len(boot_data)
    cut_percent = m.ceil((1 - confidence / 100) / 2)
    cut = round(boot_data_len * cut_percent / 100)
    middle_cuts = [cut, boot_data_len - cut]
    tail_cut = boot_data_len - cut
    print('boot_data_len: {}, cut_percent: {}, cut: {}, middle_cuts: {}, tail_cut: {}'.format(boot_data_len, cut_percent, cut, middle_cuts, tail_cut))
    head = boot_data[:cut]
    middle = boot_data[middle_cuts[0]:middle_cuts[1]]
    tail = boot_data[tail_cut:]
    main_data = pd.DataFrame(columns=['stat', 'kind'])
    main_data = main_data.append(pd.DataFrame({'stat': head, 'kind': 'head'}), ignore_index=True)
    main_data = main_data.append(pd.DataFrame({'stat': middle, 'kind': 'interval'}), ignore_index=True)
    main_data = main_data.append(pd.DataFrame({'stat': tail, 'kind': 'tail'}), ignore_index=True)
    sns.jointplot(x=main_data.index, y=main_data['stat'], kind='hex')
    plt.axhline(y=main_data.at[cut, 'stat'], color='r')
    plt.axhline(y=main_data.at[tail_cut, 'stat'], color='r')
    plt.text(cut, main_data.at[cut, 'stat'] + 0.01, 'L: {}'.format(main_data.at[cut, 'stat']), fontsize=12)
    plt.text(cut, main_data.at[tail_cut, 'stat'] + 0.01, 'U: {}'.format(main_data.at[tail_cut, 'stat']), fontsize=12)
    plot_backend = matplotlib.get_backend()
    mng = plt.get_current_fig_manager()
    if plot_backend == 'TkAgg':
        mng.resize(*mng.window.maxsize())
    elif plot_backend == 'wxAgg':
        mng.frame.Maximize(True)
    elif plot_backend == 'Qt4Agg':
        mng.window.showMaximized()
    plt.savefig('experiment.png')
    plt.show()
