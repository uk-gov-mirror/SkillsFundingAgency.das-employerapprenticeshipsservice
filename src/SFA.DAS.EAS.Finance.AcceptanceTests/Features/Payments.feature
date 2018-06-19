Feature: Payments
	In order to review my levy payments
	As an Employer
	I want to be able to see payments

Scenario: Import payments
	Given user U registered
	And user U created account A
	And standard S exists
	And provider P exists
	And account A makes a payment for standard S and period end 1718-R11
	When account A payments are imported for perid end 1718-R11
	Then account A payments should be stored