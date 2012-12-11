SourceLog
=========

SourceLog is a source control repository monitor application that tracks changes committed to a repository and displays details of each change including a side by side diff of each file changed.  The application allows the user to track changes to any number of repositories through "subscriptions" to repository paths. A number of version control systems are supported through a plugin model. Users are notified as changes are committed to the repository and new changes are marked as unread and displayed in bold.

![SourceLog Main Window](https://raw.github.com/tomhunter-gh/SourceLog/97a3d47b28e95963cdd332e67c1e2e28a0002e99/Documentation/Images/MainWindow.png "SourceLog Main Window")

[More screenshots on the wiki..](https://github.com/tomhunter-gh/SourceLog/wiki/SourceLog-Screenshots)

Implementation
--------------

The application is implemented using WPF and .NET 4. The log data is stored in a SQL Server Compact Edition 4.0 database and accessed using Entity Framework 4.1 Code First.

Supported Version Control Systems
---------------------------------

+ GitHub (via the [GitHub API](http://developer.github.com/v3/))
+ Subversion
+ Team Foundation Server 2010
+ Perforce

Install
-------

SourceLog is packaged into a ClickOnce installer and hosted on [AppHarbor](https://appharbor.com/).  The first-time installer (which includes [pre-requisites](https://github.com/tomhunter-gh/SourceLog/wiki/Application-pre-requisites)) is located here: http://sourcelog.apphb.com/Setup.exe
After the pre-requisites have been installed, the application should be installed and updated automatically but the current version can be installed at any time using the following URL: http://sourcelog.apphb.com/SourceLog.application

Status
------

The application is currently a work in progress and the current focus is on a [version 1](https://github.com/tomhunter-gh/SourceLog/issues?milestone=1) release. 

Feedback
--------

Feedback is very much welcome and encouraged across, but not limited to, the following areas:

+ The GitHub project page / [AppHarbor host page](http://sourcelog.apphb.com/)
+ The install experience
+ The features/functionality of the application
+ The design/architecture of the application
+ The coding style and 



- - -

SourceLog is inspired by [CommitMonitor](http://tools.tortoisesvn.net/CommitMonitor.html).