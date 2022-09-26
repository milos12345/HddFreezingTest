# HddFreezingTest

Test scenario:
- Power Options set to turn off disks in 1 minute (for faster testing)
- USB-Attached RAID with 4-HDDs, but it should happen with any HDD

Minimum example of how DirectoryExists or GetFileAttributes freezes UI despite being on a different thread, if the USB-attached disk is sleeping

You can still browse a few random dirs but eventually it will freeze on GetFileAttributes

Sometimes it will wake disk up without freezing.

Example output:
```
H:\Photos
26/09/2022 11:32:19 H:\Photos exists on thread 11
26/09/2022 11:32:19 Got 6 dirs on thread 11
26/09/2022 11:32:51 Second .Exists for H:\Photos\2022 on thread 11
H:\Photos\2022
```
See the difference between :19 and :51 while 4 disks of the RAID were spinning up. UI was frozen half of this time.

I am relying on hearing HDD spin-down and spin-up to know when it is sleeping/awake as it is loud anyway
