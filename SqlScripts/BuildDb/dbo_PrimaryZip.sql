CREATE FUNCTION [dbo].[PrimaryZip] ( @pid int )
RETURNS nvarchar(11)
AS
	BEGIN
declare @zip nvarchar(11)
select @zip =
	case AddressTypeId
			when 10 then f.ZipCode
			when 30 then p.ZipCode
	end
from dbo.People p join dbo.Families f on f.FamilyId = p.FamilyId
where PeopleId = @pid

	RETURN @zip
	END
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
