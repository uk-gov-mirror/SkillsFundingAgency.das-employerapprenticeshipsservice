SET NOCOUNT ON;

IF EXISTS(SELECT	1 
			FROM	INFORMATION_SCHEMA.COLUMNS
		    WHERE	TABLE_SCHEMA = 'employer_account')
BEGIN

	BEGIN TRY

		BEGIN TRANSACTION;

			DECLARE @CountMembershipMissingUas int
		
			SELECT @CountMembershipMissingUas = count(*) FROM [employer_account].[Membership] m
			FULL OUTER JOIN [employer_account].[UserAccountSettings] uas
			ON m.AccountId = uas.AccountId
			AND m.UserId = uas.UserId
			WHERE uas.Id IS NULL

			PRINT Convert(varchar, @CountMembershipMissingUas) + ' membership records with no associated useraccountsettings';

			PRINT 'Clearing ' + Convert(varchar, @CountMembershipMissingUas) + ' membership records with no associated useraccountsettings';

			DELETE m FROM [employer_account].[Membership] m
			INNER JOIN (
				SELECT m.AccountId, m.UserId FROM [employer_account].[Membership] m

				FULL OUTER JOIN [employer_account].[UserAccountSettings] uas
				ON m.AccountId = uas.AccountId
				AND m.UserId = uas.UserId
				WHERE uas.Id IS NULL
			) incomplete
			ON m.AccountId = incomplete.AccountId
			AND m.USerId = incomplete.userId



			DECLARE @CountUasMissingMembership int
		
			SELECT @CountUasMissingMembership = count(*) 
			FROM [employer_account].[Membership] m
			FULL OUTER JOIN [employer_account].[UserAccountSettings] uas
			ON m.AccountId = uas.AccountId
			AND m.UserId = uas.UserId
			WHERE m.AccountId IS NULL

			PRINT Convert(varchar, @CountUasMissingMembership) + ' useraccountsettings records with no associated membership';

			PRINT 'Clearing ' + Convert(varchar, @CountMembershipMissingUas) + ' useraccountsettings records with no associated membership';

			DELETE uas FROM [employer_account].[UserAccountSettings] uas
			WHERE uas.Id in
			(
				SELECT uas.Id 
				FROM [employer_account].[Membership] m
				FULL OUTER JOIN [employer_account].[UserAccountSettings] uas
				ON m.AccountId = uas.AccountId
				AND m.UserId = uas.UserId
				WHERE m.AccountId IS NULL
			)

		COMMIT TRANSACTION;

		PRINT '<<OK: TranCount:' + convert(varchar(10), @@TranCount);

	END TRY
	BEGIN CATCH

		PRINT 'Error: could not clean incomplete user account bindings';
		PRINT ERROR_MESSAGE();

		IF @@TRANCOUNT > 0
		BEGIN
			PRINT 'Rolling back transaction';
			ROLLBACK TRAN;
		END;

		PRINT '<<Error: TranCount:' + convert(varchar(10), @@TranCount);

		THROW;

	END CATCH;
END;

GO
