# Figaro.Web.ApplicationServices
The Membership, Authentication, Role and Profile classes contain methods that let you log on users, check which roles the current user belongs to, and retrieve profile properties for the user out of the Figaro embedded XML database library.

This library is currently configured to use the Figaro DS x86 library, though the source code wil accomodate the CDS and TDS editions as well through the use of build constants - look for the #if statements. All XQuery is stored in resource files.
