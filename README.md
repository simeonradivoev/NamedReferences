A small utility for the unity editor that shows references to and from the selected game object in the hierarchy window.

## How to Use
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
