<DIV ID="header">

<A HREF="[@out(nav+home+href);]"><IMG SRC="[@out(site+logo);]" WIDTH="225px" HEIGHT="75px" /></A>

<DIV ID="login"><DIV ID="userimg">

[@if(@exists(user));]
<IMG WIDTH="50px" HEIGHT="50px" SRC="[@out(user+avatar);]" /></DIV>Welcome, [user+name]<BR />
</DIV>
[@else;]
<IMG WIDTH="50px" HEIGHT="50px" SRC="[@out(site+avatar);]" /></DIV>Welcome, Guest<BR />
<A HREF="[@out(nav+login+href);]">Login</A><A HREF="[@out(nav+register+href);]">Register</A>
</DIV>
[@end;]
</DIV>

<DIV ID="nav"><UL>
<LI><A CLASS="[@out(nav+home+sel);]" HREF="[@out(nav+home+href);]">Home</A></LI>
<LI><A CLASS="[@out(nav+slist+sel);]" HREF="[@out(nav+slist+href);]">Server List</A></LI>
<LI><A CLASS="[@out(nav+help+sel);]" HREF="[@out(nav+help+href);]">Help</A></LI>
<LI><A CLASS="[@out(nav+appeal+sel);]" HREF="[@out(nav+appeal+href);]">Ban Appeal</A></LI>
<LI><A CLASS="[@out(nav+blist+sel);]" HREF="[@out(nav+blist+href);]">Banlist</A></LI>
</UL>

</DIV>
