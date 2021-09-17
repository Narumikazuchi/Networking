![Logo](../master/logo.png)

# Utility Library
This library originally contained all classes that I used in my projects more than once. I made them universal instead of being hardcoded for the project in question and added them to the library in order to reuse them whenever needed. With time the library grew and not all aspects were needed in every project. That's when I decided to split them into organized individual pieces and also publish them on github as well as nuget.org.

# Networking
This library was created with future additions in mind. While currently only holding the very humble MacAddress struct, I plan to add more stuff when I design something fitting while working on other projects.
# Sockets
A result of my work with server-client type projects where communication over an internet connection is needed. I originally build this while not knowing of the TcpClient and UdpClient classes, but since then I readiliy used it. Therefore this will probably one of the libraries that are guaranteed to be held up to date.
  
## Installation
### Networking
[![NuGet](https://img.shields.io/nuget/v/Narumikazuchi.Networking.svg)](https://www.nuget.org/packages/Narumikazuchi.Networking)  
The installation can be simply done via installing the nuget package or by downloading the latest release here from github and referencing it in your project.
### Sockets
[![NuGet](https://img.shields.io/nuget/v/Narumikazuchi.Networking.Sockets.svg)](https://www.nuget.org/packages/Narumikazuchi.Networking.Sockets)  
The installation can be simply done via installing the nuget package or by downloading the latest release here from github and referencing it in your project.
