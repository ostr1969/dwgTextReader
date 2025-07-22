set DWDCRAWLER_FOLDER=C:\install\Pdfs
cd C:\Users\barako\source\repos\ACadSharp\src\FolderMon\bin\Debug\net8.0
sc delete FolderMonitorService 
sc create FolderMonitorService binPath= C:\Users\barako\source\repos\ACadSharp\src\FolderMon\bin\Debug\net8.0\FolderMon.exe
sc start FolderMonitorService 

rem sc stop FolderMonitorService 

