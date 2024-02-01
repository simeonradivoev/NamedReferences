A small utility for the unity editor that shows references to and from the selected game object in the hierarchy window.
Was created mainly to address, me always losing what references what after I come back to a project after a while.

## How to Use
You can skip this and only use the visualizer of references that alone comes in handy.
To use it add a `NamedReferenceAttribute` to a serialized field of a game object or component. During runtime references will be assigned automatically. Here are some examples: 
````csharp
[NamedReference("#Test")]
public GameObject Reference;

[NamedReference("#Test",true)]
public GameObject DirectReference;
````

To use at runtime there are helper extension methods for transforms like this:
````csharp
transform.FindAnyChild("#TestChild");
transform.FindAnyChild<Image>("#TestChildImage");
transform.FindAnyChildDirect("#TestDirectChild");
transform.FindAnyChildDirect<Slider>("#TestDirectSlider");
````

There are 2 types of children:
### Direct (Only children that are direct parents of the root will be selected)
* Root
  * #Child_1
    * #TestChild
  * #TestChild

In the example above searching for the `#TestChild` from root, only the second `#TestChild` will be valid as it is not a part of another parent starting with the `#` character. This character is used as an indicator.
### Any
This will ignore any parent that start with the `#` character

## Screenshots
Green is references to the selected object and yellow is references from the selected object.

![Unity_dhQipXxUup](https://github.com/simeonradivoev/NamedReferences/assets/9950198/b3e9c938-b5d2-4610-bc64-f110c90e3e54)
![Unity_RQyTmC8qNQ](https://github.com/simeonradivoev/NamedReferences/assets/9950198/10921e33-c362-4af1-bd74-2b4818f5bfac)
