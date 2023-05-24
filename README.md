# EventSystem

## How to Initialize

1. Import in your project.
2. Replace relevant information found at the top of EventEditor.cs.
3. Enjoy!

## How to Use

**1. Determine what type of event system that you need. Two methods are possible with this package. The only thing that changes between both is how the dispatcher is accessed.**
* Global: Anyone can access the singleton event dispatcher, send and receive events through it.
* Hierarchal: Listeners need to be a child object of the dispatcher to register, send and receive events through it.

**2. Inside the Examples folder are, well, examples of how to make implementations of EventBase (which all your events will use) as well as a default Dispatcher (which is the singleton that sends events).**

**3. If you've properly edited the relevant informations at the top of EventEditor.cs, you should be able to open the editor under Data/Event Editor (in Unity). If you've made your EventBase properly, when creating a new event with the editor, it should assign your homemade EventBase as its Target.**

**4. For each event, here are the important informations you have to give to the editor.**
* Namespace: What namespace the event should be made in.
* Imports: All the 'using' statements the event needs.
* Name: The event name. My go-to for this is "{Prefix}{What event touches}{Verb}Event". So an event of type CharacterEventBase that sets position would be CharacterPositionSetEvent. Doing it this way makes sorting very easy. All Character events start with Character, all Position events start with CharacterPosition, etc.
* Target: If you have multiple EventBase implementations in your project, you can select the one you wish to inherit here.
* Enable Custom Code?: Sometimes, you wish to add code that isn't generated, but which persists even if you regenerate the event. It'll add a spot in the event script where code can be added. Code in there should never be deleted by the editor.
* Do we send logs?: If true, it shows an extra box where you can set a comment that'll be sent as a log whenever the event is requested.
* Variables: All the variables the event carries with it.
  * Params: If set to true, adds the 'params' keyword to where the variable is set. (https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/params)
  * Type: Variable type.
  * Name: Variable name. Names here should start with a capital letter. It'll cause an error otherwise in one of the generated function.
  * Def.Value: Default Value given to the variable. Also makes it so it has a default value when preparing the event, so you don't need to give a value if you don't want to.

**5. Clicking Generate will create the event with the given values and import it to your project.**

**6. Now that you have your own event type, your own dispatcher and even your own event implementation, you can use the system! Please refer to RandomExampleClass.cs in the Examples folder for a showcase of how to use all your new tools.**
