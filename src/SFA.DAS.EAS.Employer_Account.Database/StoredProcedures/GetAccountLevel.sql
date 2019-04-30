CREATE PROCEDURE [employer_account].[GetAccountLevel]	
	@accountId BIGINT
	
AS
IF NOT EXISTS(SELECT * FROM [employer_account].[Membership] WHERE AccountId = @accountId) 
	BEGIN
		SELECT 'BasicAccount' --'THIS IS ACCOUNT LEVEL 1 - BASIC ACCOUNT'
	END
ELSE 
	BEGIN
		DECLARE @numberOfOrganisations BIGINT,
				@numberOfPendingAgreements BIGINT,
				@numberOfSignedAgreements BIGINT

		DECLARE @numberOfPayeSchemes BIGINT = 
			(
				SELECT COUNT(*)
				FROM  [employer_account].[AccountHistory] 
				WHERE AccountId = @accountId
				AND   RemovedDate IS NULL
			)

		SELECT @numberOfOrganisations = COUNT(*),
			   @numberOfPendingAgreements =	SUM(CASE WHEN PendingAgreementId IS NULL THEN 0 ELSE 1 END),
			   @numberOfSignedAgreements = SUM(CASE WHEN SignedAgreementId IS NULL THEN 0 ELSE 1 END)
		FROM [employer_account].[User] u
		JOIN [employer_account].[Membership] m ON u.id = m.UserId
		JOIN [employer_account].[Account] a ON m.AccountId = a.Id
		LEFT JOIN [employer_account].[AccountLegalEntity] ale ON a.Id = ale.AccountId
		LEFT JOIN [employer_account].[LegalEntity] le ON le.Id = ale.LegalEntityId
		WHERE a.Id = @AccountId
		AND   Deleted IS NULL
		GROUP BY a.Id

		IF @numberOfPayeSchemes = 0 
			BEGIN
				SELECT 'AccountHasNoPAYE' --'THIS IS ACCOUNT LEVEL 2 - ACCOUNT HAS NO PAYE'
			END

		ELSE IF @numberOfPendingAgreements = @numberOfOrganisations
			BEGIN
				SELECT 'AccountHasPAYEAndOrganisationsButNoSignedAgreements'  --'THIS IS ACCOUNT LEVEL 3 - ACCOUNT HAS PAYE AND ORGANISATIONS BUT NO SIGNED AGREEMENTS'
			END

		ELSE IF @numberOfSignedAgreements = @numberOfOrganisations
			BEGIN
				SELECT 'AccountHasPAYEAndAllOrganisationsHaveSignedAgreements' --'THIS IS ACCOUNT LEVEL 5 - ACCOUNT HAS PAYE AND ALL ORGANISATIONS HAVE SIGNED AGREEMENTS'
			END

		ELSE IF @numberOfPendingAgreements > 0
			BEGIN
				SELECT 'AccountHasPAYEAndSomeOrganisationsHaveSignedAgreements' --'THIS IS ACCOUNT LEVEL 4 - ACCOUNT HAS PAYE AND SOME ORGANISATIONS HAVE SIGNED AGREEMENTS'
			END

		--SELECT @numberOfOrganisations AS '@numberOfOrganisations', @numberOfPendingAgreements AS '@numberOfPendingAgreements', @numberOfSignedAgreements AS '@numberOfSignedAgreements', @numberOfPayeSchemes AS '@numberOfPayeSchemes'
		--todo what about scenario where account has PAYE and organisations but no pending agreements - can this ever happen?
	END