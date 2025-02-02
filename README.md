SourceLog
=========

SourceLog is a source control repository monitor application that tracks changes committed to a repository and displays details of each change including a side by side diff of each file changed.  The application allows the user to track changes to any number of repositories through "subscriptions" to repository paths. A number of version control systems are supported through a plugin model. Users are notified as changes are committed to the repository and new changes are marked as unread and displayed in bold.

![SourceLog Main Window](https://raw.github.com/tomhunter-gh/SourceLog/master/Documentation/Images/MainWindow.png "SourceLog Main Window")

[More screenshots on the wiki..](https://github.com/tomhunter-gh/SourceLog/wiki/SourceLog-Screenshots)

Implementation
--------------

The application is implemented using WPF and .NET 4. The log data is stored in a SQL Server Compact Edition 4.0 database and accessed using Entity Framework 4.1 Code First.

Please see my blog post for further information: http://www.unhandledexception.info/sourcelog/

Supported Version Control Systems
---------------------------------

+ Git
+ GitHub (via the [GitHub API](http://developer.github.com/v3/))
+ Mercurial (thanks to [cl3m](https://github.com/cl3m)!)
+ Subversion
+ Team Foundation Server 2010
+ Perforce

Install
-------

SourceLog is packaged into a ClickOnce installer and hosted on [AppHarbor](https://sourcelog.apphb.com/).

Status
------

The application is now fairly stable.  There are a number of enhancements suggested on [the issues list](https://github.com/tomhunter-gh/SourceLog/issues?state=open).

Feedback and Contributions
--------

Feedback and suggestions are very much welcome and I would be keen to get input on all the following areas:

+ The GitHub project page / [AppHarbor host page](http://sourcelog.apphb.com/)
+ The install experience
+ The features and functionality of the application
+ The design and architecture of the application
+ The coding style and conventions in use

I would also be very happy to accept pull requests and collaborate with others on further development of the application.

tom@unhandledexception.info

- - -

SourceLog is inspired by [CommitMonitor](http://tools.tortoisesvn.net/CommitMonitor.html).
