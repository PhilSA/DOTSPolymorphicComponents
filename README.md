
# DOTS Polymorphic Components

## How to install
Simply download the .unitypackage in the "Releases" section of this repository, and import into your DOTS-compatible project (requires Entities package)


## How to use

### Create the Polymorphic Component Definition
Create an interface that has the `PolymorphicComponentDefinition` attribute. This interface will define the functions that will be common to all possible component types belonging to your new polymorphic component. See an example [here](https://github.com/PhilSA/DOTSPolymorphicComponents/blob/master/Assets/_Samples/PolymorphicTest/IMyPolyComp.cs). You are allowed to pass any sort of parameters to those functions (including by "ref", "in", and "out"), but you cannot make those functions have a return type

The `PolymorphicComponentDefinition` attribute takes 3 parameters:
* the name of the generated component
* the file path of the generated component
* the additional usings that the generated component should have

### Create the specific components
Create structs implementing that interface. All structs implementing the same `PolymorphicComponentDefinition` interface will represent the various types that your polymorphic component can assume the role of. See an example [here](https://github.com/PhilSA/DOTSPolymorphicComponents/blob/master/Assets/_Samples/PolymorphicTest/CompA.cs) and [here](https://github.com/PhilSA/DOTSPolymorphicComponents/blob/master/Assets/_Samples/PolymorphicTest/CompB.cs)

### Generate the PolymorphicComponent code
In the Unity editor, in the top menu bar, select "Tools > Generate PolymorphicComponents". This will generate the component code with the name & path defined in your `PolymorphicComponentDefinition` attribute.

You only really have to generate again when you either add new specific component types, or add new functions to your polymorphic component interface

### Create authoring components
You can now create an authoring component for each of your specific component types. The component you author in the inspector will be the specific struct type, but the component you will add to the entity will be the generated polymorphic component ([example](https://github.com/PhilSA/DOTSPolymorphicComponents/blob/master/Assets/_Samples/PolymorphicTest/CompAAuthoring.cs))
