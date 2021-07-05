
MODULES=Document.netmodule Controller.netmodule Ed.netmodule

CSCOPTS=-reference:/usr/local/microsoft/powershell/7/System.Management.Automation.dll

all: ed.dll

ed.dll: modules
	csc -out:ed.dll $(CSCOPTS) $(MODULES)

Controller.netmodule: Controller.cs 
	csc $(CSCOPTS) -target:module -addmodule:Document.netmodule $<

modules: $(MODULES)

%.netmodule: %.cs
	csc  $(CSCOPTS) -target:module $<

