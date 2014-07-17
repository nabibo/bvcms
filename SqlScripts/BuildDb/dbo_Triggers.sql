
CREATE VIEW [dbo].[Triggers]
AS
SELECT TOP (100) PERCENT Tables.name AS TableName, Triggers.name AS TriggerName, Triggers.crdate AS TriggerCreatedDate, Comments.text AS Code
FROM  sys.sysobjects AS Triggers INNER JOIN
               sys.sysobjects AS Tables ON Triggers.parent_obj = Tables.id INNER JOIN
               sys.syscomments AS Comments ON Triggers.id = Comments.id
WHERE (Triggers.xtype = 'TR') AND (Tables.xtype = 'U')
ORDER BY TableName, TriggerName
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
