CREATE VIEW [dbo].SpouseOrHeadWithEmail AS
	SELECT pid PeopleId FROM
	(
		SELECT 
			ht.EmailAddress HisEmail,
			st.EmailAddress HerEmail,
			CASE WHEN st.EmailAddress IS NOT NULL THEN st.PeopleId ELSE ht.PeopleId END pid,
			ISNULL(st.EmailAddress, ht.EmailAddress) email
		FROM
		(
			SELECT h.PeopleId, h.EmailAddress, h.SpouseId
			FROM dbo.People h
			JOIN dbo.Families f ON f.FamilyId = h.FamilyId AND h.PeopleId = f.HeadOfHouseholdId
		) ht
		LEFT JOIN
		(
			SELECT 
				s.PeopleId, 
				s.EmailAddress
			FROM dbo.People s
			JOIN dbo.Families f ON f.FamilyId = s.FamilyId AND s.PeopleId = f.HeadOfHouseholdSpouseId

		) st ON st.PeopleId = ht.SpouseId
	) tt
	WHERE LEN(email) > 0
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
