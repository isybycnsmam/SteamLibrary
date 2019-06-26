# SteamLibrary
You can login to steam via this library or join to steam login based sites

This library supports
 - logging into steam
 - generating two factor code
 - login via steam
 - and captcha support while loging in
 
 TODO:
- Change nick
- Register ?
- Change avatar
- Change profile appearance
- Steam mobile trade token
- download eq
- cloudflare support




Useage:
//create account object
var account = new SteamClient();

//token = steam shared secret(28 chars) if you have two step verification
//token is also generated token and then you dont need to know secret
account.Login("username", "password", "token")

////////now you are loged if any exception was not thrown//////////
//for now you can only
