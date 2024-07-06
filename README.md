Resolve dependencies
Your task is to analyze software package dependencies. A system with package
dependencies is described like this:
2
A,1
B,1
3
A,1,B,1
A,2,B,2
C,1,B,1
● The first line is the number (N) of packages to install.
● The next N lines are packages to install. These are in the form p,v where p is a
package and v is a version that needs to be installed.
● The next line is the number (M) of dependencies
● The following M lines are of the form p1,v1,p2,v2 indicating that package p1 in
version v1 depends on p2 in version v2.
● Packages and version are guaranteed to not contain ',' characters.

If more than one version of a package is required the installation is invalid. Your task is
to check if installing the packages (along with all packages required by
dependencies) is valid.
The sample input above is valid, but the sample input below is invalid as A,1 requires
B,1 and we are trying to install B,2.
2
A,1
B,2
3
A,1,B,1
A,2,B,2
C,1,B,1
