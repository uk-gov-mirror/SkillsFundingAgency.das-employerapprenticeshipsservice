Feature: TransferConnectionInvitations
	In order to ensure a receiver can only initiate connections under certain conditions
	I want to be told the outcome of initiating the transfer connection invitation
Scenario: Try connect to a sender with no Transfer Balance
	Given user Dave registered
	And user Dave2 registered
	And user Dave3 registered
	And user Dave created account A
	And user Dave2 created account B
	And user Dave3 created account C
	And account B is a sender to account C
	Then we are notified that there was a failure when user Dave of account A sends a transfer connection invitation to account B
	#Then the sender is informed the receiver is already a sender

Scenario: Try connect to a sender with Transfer Balance
	Given user Dave registered
	And user Dave2 registered
	And user Dave3 registered
	And user Dave created account A
	And user Dave2 created account B
	And user Dave3 created account C
	And account B is a sender to account C
	And a transfer allowance of 1000.00 is set for Paye Scheme "123/456" for account A
	#And account A has a signed agreement enabling transfers
	When user Dave of account A sends a transfer connection invitation to account B
	Then we are notified that the connection has been sent
	#Then the sender is informed the receiver is already a sender
