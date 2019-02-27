An integration for passing HubSpot contacts into Constant Contact automatically.

Allow user to determine into which Constant Contact lists a given contact should be added 
based on their 'Contact Type' (a custom field in HubSpot.)

DEAL CHANGE EVENT:
	1) Receive deal change event from HubSpot
	2) Grab corresponding deal along with contacts
		a) Need to pass deal name along with contact into 'Project' field in Constant Contact

CONTACT SYNCING
	1) Pull current info from HubSpot
	2) Validate
		a) Should have at least one e-mail address
		b) Should have a contact type (custom field in both HubSpot and Constant Contact)
		c) Contact type should have associated lists
	3) Compare current HubSpot data to most recent data in database to determine what kind
	   of operation is needed
		a) If contact has been previously synced and info is the same, do nothing
		b) If contact has not been synced or info does not match DB info, 
		   update in Constant Contact
			i) If not previously synced, check if contact exists in ConstantContact (by e-mail) to
			   determine if needs to be added or updated

LOGGING VIEWS
	- Deal change event history
	- See contacts that were not transferred to Constant Contact after a deal change event
		- Allow trigger of 're-sync' for a contact

USER ADMIN
	- Determine which deal stages should trigger a contact transfer
	- Determine which contact types should be transferred to which Constant Contact lists
	- Manage API keys (integration site, HubSpot, Constant Contact)