# Readme
# About
Foxie adVentures is a short conceptual game developed by Julio Maciel in order to show off game development skills using Unity and C#. The objective was to replicate some notorious features from games developed by the Foxie Venture studio, such as Horse Riding Tales, and Wolf Tales. You can check this out and play on [itch.io page](https://juketo.itch.io/foxie).

As a challenge, it was defined to limit the quantity of third-party code to the minimum possible, therefore, besides using the Unity engine, and its very necessary TextMeshPro built-in plugin, no other internal or external code library was utilized. Consequently, all code was designed and written from the scratch.

## History
Foxie adVentures is a history about Jay, a former employee from the Boring Industry. At some moment, Jay realized his life was meanless, and it had reached the time to finally pursue his dreams. Jay moves to the Hills, where he founded his beloved farm. However, the Boring Industry, which desperately needs every developer, will not give up on him, and they will try to bring Jay back.
You control Foxie, one of the many friendly wild animals Jay took in, and your mission will be to assist and support Jay to stay where he is meant to be.

## Mimicking Horse Riding Tales
As previously mentioned, this game simulates several features from Horse Riding Tales, the most successful Foxie Venture game. The images below illustrate some of these similarities.

### Free camera control and mouse-based player movement
![control_camera](https://user-images.githubusercontent.com/12513988/189030843-c1c19633-a139-4407-9968-4ef63ca78b74.png)
### 2D UI control and IA
![attack_snake](https://user-images.githubusercontent.com/12513988/189030826-4f38143d-b8ee-4386-8578-9496685c62ac.png)
### 2D UI control over 3D scene
![attack boring](https://user-images.githubusercontent.com/12513988/189030803-6ee9d4df-886c-4b79-bd50-30977b8fe080.png)

## FAQ
- Unity version
	- 2021.3.8f LTS - URP
- Time spent
	- Around one month of full work. The first commit, however, was submitted on May/22, which would increase the dev time to 3/4 months, but there were several weeks of pauses and resumes. 
- Unity tools/features
	- ScriptableObjects, URP Volume, Animators, Unity Test, Coroutines, Colliders, State Machines, Pooling, Particle System, NavMesh, URP Light, UI Components, Value types structures, etc...
- 3D Models
	- From third-party art companies. Mostly low-poly models are from Synty studios.
- Animations
	- All animators were created from scratch. No external scripts were reutilized. Some animation clips were edited to remove all root motion to demonstrate full control of the process.
- Tests
	- Unity PlayMode tests were implemented to support the dialogue balloon feature. Check BalloonPositionTests.cs
- Code documentation
	- Instead of several extra commented lines, explaining what the next line will do, this project uses the principle of self-explained code. For instance, a named variable (var), explains what its code does. The same applies to private methods. 
- Mobile version
	- Not yet. The main objective was to develop an easily accessible game [playable on the browser and Windows](https://juketo.itch.io/foxie). Besides reading player touch input control, however, there should not be much difficulty to make the game work on Android, for instance. 
