For Demos, First you need to create Main Azure Resources such as: 
                                                                1. Azure Data Factory
                                                                
                                                                2. Azure Functions
                                                                
                                                                3. Azure Logic Apps
                                                                
                                                                4. Azure Key Vault
                                                                
                                                                5. Azure Storage Account
                                                                
                                                                6. Azure SQL Server
                                                         
 Then, you can add/import repo from this Github repo so your ADF has Pipeline, Datasets, LinkedServices and etc from this Github Repo.
 
 Make sure your Functions App has Functions. Also Logic Apps has trigger and actions to Email Functionality.
 
 Make sure Key Vault has access policy for ADF so ADF can access secrets from Key Vault for Connection string(Azure SQL Database) and Access Key (Storage)
 
 Also Add secret to Key Vault for Azure SQL Database(Connection Key), Azure Storage Account(Access Key) and Azure Functions(Function Key)
 
 Also Make sure Data Factory has contributon access in Azure Storage Account.
 
 Finally, make sure you have Container in Azure Storage Account and it has files.
 
 There are following containers needed.You can find files https://github.com/alpaBuddhabhatti/PASSSUMMIT2022/edit/ADF_AF_ALP/
 
 1. file-in ( *.jpg & *.csv)
 2. input container (Having .Jpg files)
 3. output container (having resized jpg files automatically by Azure Function as soon as jpg file arrives in input container)
 4. Also, Create Logic Apps.(you find Logic Apps ARM templates and Parameters template(Json file))
 5. Create Azure Function App projects using Visual studio 2019 or Visual code. Azure Function code in (2 C# file having Azure Functions). In your solution you need to add .sdk for Blob bindings. Also Deploy Azure fucntions to Azure.
    
