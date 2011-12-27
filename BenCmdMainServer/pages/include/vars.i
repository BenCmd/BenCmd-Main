@set("sys+createOnNull", true);

# CONFIG
@set("site+http", "http://bencmd.servehttp.com/");
@set("site+logo", site+http $ "img/logo.png");
@set("site+avatar", site+http $ "img/default.png");

# NAV LINKS
@set("nav+home+href", site+http $ "index.html");
@set("nav+slist+href", site+http $ "special/servers.html");
@set("nav+help+href", site+http $ "help.html");
@set("nav+appeal+href", site+http $ "special/appeal.html");
@set("nav+blist+href", site+http $ "special/banlist.html");
@set("nav+login+href", site+http $ "special/login.html");
@set("nav+register+href", site+http $ "special/register.html");

@if(@equal(sys+page, nav+home+href)); @set("nav+home+sel", "selected"); @else; @set("nav+home+sel", ""); @end;
@if(@equal(sys+page, nav+slist+href)); @set("nav+slist+sel", "selected"); @else; @set("nav+slist+sel", ""); @end;
@if(@equal(sys+page, nav+help+href)); @set("nav+help+sel", "selected"); @else; @set("nav+help+sel", ""); @end;
@if(@equal(sys+page, nav+appeal+href)); @set("nav+appeal+sel", "selected"); @else; @set("nav+appeal+sel", ""); @end;
@if(@equal(sys+page, nav+blist+href)); @set("nav+blist+sel", "selected"); @else; @set("nav+blist+sel", ""); @end;

# USER
@if(@nequal(sys+sender+login, ""));
	@set("user+name", sys+sender+login);
	@set("user+email", sys+sender+email);
	@set("user+avatar", "http://www.gravatar.com/avatar/" $ @md5(user+email) $ "?s=50&d=" $ @encode(site+avatar) $ "&r=pg");
@end;

@set("sys+createOnNull", false);

