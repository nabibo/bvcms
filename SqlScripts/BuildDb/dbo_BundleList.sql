

CREATE VIEW [dbo].[BundleList]
AS
SELECT
	h.BundleHeaderId,
	MAX( ht.Description ) AS HeaderType,
	MAX(h.DepositDate) AS DepositDate,
	MAX(ISNULL(h.TotalCash,0)) + MAX(ISNULL(h.TotalChecks,0)) + MAX(ISNULL(h.TotalEnvelopes,0)) AS TotalBundle,
	MAX( h.FundId ) AS FundId,
	MAX( f.FundDescription ) AS Fund,
	MAX( bs.Description ) AS Status, 
	MAX( h.BundleStatusId ) AS [open],
	MAX( c.PostingDate ) AS PostingDate,
	MAX( c.TotalItems ) AS TotalItems,
	MAX( c.ItemCount ) AS ItemCount,
	MAX( c.TotalNonTaxDed ) AS TotalNonTaxDed
FROM BundleHeader AS h
INNER JOIN lookup.BundleHeaderTypes AS ht ON h.BundleHeaderTypeId = ht.Id
INNER JOIN lookup.BundleStatusTypes AS bs ON h.BundleStatusId = bs.Id
LEFT JOIN (
	SELECT d.BundleHeaderId,
		MAX(c.PostingDate) AS PostingDate,
		ISNULL( SUM(c.ContributionAmount), 0 ) AS TotalItems,
		COUNT(c.ContributionId) AS ItemCount,
		SUM( CASE WHEN c.ContributionTypeId = 9 or f.NonTaxDeductible = 1 THEN c.ContributionAmount ELSE 0 END ) AS TotalNonTaxDed
	FROM BundleDetail AS d
	INNER JOIN Contribution AS c ON c.ContributionId = d.ContributionId
	INNER JOIN ContributionFund AS f ON c.FundId = f.FundId
	GROUP BY d.BundleHeaderId
	) AS c ON c.BundleHeaderId = h.BundleHeaderId 
LEFT JOIN ContributionFund AS f ON h.FundId = f.FundId
WHERE h.RecordStatus = 0
GROUP BY h.BundleHeaderId



GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
