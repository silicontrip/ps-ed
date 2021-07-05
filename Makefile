
MODULES=Document.netmodule Controller.netmodule Ed.netmodule

CSCOPTS= -lib:/usr/local/microsoft/powershell/7 \
-reference:System.Management.Automation.dll

#\;System.Runtime.dll
#\;System.Private.CoreLib.dll
#\;mscorlib.dll

all: ed.dll

ed.dll: modules
	csc -out:ed.dll $(CSCOPTS) $(MODULES)

Controller.netmodule: Controller.cs Document.netmodule
	csc $(CSCOPTS) -addmodule:Document.netmodule -target:module  $<

modules: $(MODULES)

%.netmodule: %.cs
	csc  $(CSCOPTS) -target:module $<

