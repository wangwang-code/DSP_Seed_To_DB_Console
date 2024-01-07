# DSP_Seed_To_DB_Console
DSP_Seed_To_DB_Console暴力多线程版
修改自`https://gitee.com/stanleylam19/dsp-seeds-to-db-tools/`

原版
```
请输入起始种子号:
0
请输入结束种子号
10000
请输入星系数
64
Begining to generate seed to DB
9999
done耗时: 00:02:30.3845067
```
暴力多线程版
```
T
请输入起始种子号:
0
请输入结束种子号
10000
请输入星系数
64
Begining to generate seed to DB
done耗时: 00:01:41.7325673
```

受SQLite限制，无法实现真正的多线程，实际上由线程之间抢占SQLiteCommand，对SQL命令缓存，每创建出20个线程后，主线程将与其他线程抢占SQLiteCommand，将内存里的SQL命令执行入硬盘（防止内存溢出）（硬盘危）

由于每个线程完成时间不一致，所以SQL内的数据是乱序的

（不保证数据完整、准确）
