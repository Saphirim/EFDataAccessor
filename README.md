# EFDataAccessor
A simple, mostly async DataAccessor for Entity Framework. Allowing standard CRUD Operations in complex data models with parallel database calls.

Including a sample project and corresponding unit tests.

This project and the idea behind relies heavily on serveral blog posts from Rob Sanders (http://stackexchange.com/users/9903/robs). 

Here's one of his featured articles about the DataAccessor: http://sanderstechnology.com/2014/update-entity-framework-generic-data-access/12959/#.VktLe_kvdhE

The pure example from Mr. Sanders was modified, refactored and made async where it was possible. Some bugs were fixed also.


Please refere to the sample project on how to use the DataAccessor in your own Project.

To run the unit tests, MSSQLLOCALDB should be installed and accessible by system authentification:

https://msdn.microsoft.com/de-de/sqlserver2014express.aspx

Feel free to fork, improve or make comments to this project and help to fight the nasty problems about threadsafety, detached many-to-many relations and other points of interest while handling the Entity Framework.

For pracical usage, make sure, your DbContext is created like this:

var context = new DataContext

                {
                
                    Configuration =
                    
                    {
                    
                        ProxyCreationEnabled = false,
                        
                        AutoDetectChangesEnabled = true,
                        
                        LazyLoadingEnabled = false
                        
                    }
                    
                };
