# About R7.News

[![BCH compliance](https://bettercodehub.com/edge/badge/roman-yagodin/R7.News)](https://bettercodehub.com/)

The goal of *R7.News* project is to provide a streamlined news subsystem for DNN Platform, 
which would take advantage from tight CMS integration and combinatorial approach to news article content authoring.

## License

[![AGPLv3](https://www.gnu.org/graphics/agplv3-155x51.png)](https://www.gnu.org/licenses/agpl-3.0.html)

The *R7.News* is free software: you can redistribute it and/or modify it under the terms of 
the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, 
or (at your option) any later version.

## Main features outline:

- [x] Mark news entry with tags and use them to provide granular thematic views.
- [x] Cast any page into news article by placing *R7.News.Agent* module onto it.
- [x] Streamline new articles creation by adding *R7.News.Agent* module into the page template.
- [x] Allow users to [discuss](#discuss) news on forum.
- [ ] Keep news article page settings in sync with corresponding news entry properties.
- [ ] Export news into RSS/Atoms feeds.
- [ ] DDRMenu integration (to display recent news in the menu).

### Future releases may also include:

* News sharing between portals inside the portal group.
* Content workflow (at least Draft/Publish).

## System requirements

* DNN Platform v8.0.4.

## Install

1. Download and install [dependencies](#dependencies) first.
2. Download latest install package from releases.
3. Install it as usual from *Host &gt; Extensions*.

### <a name="dependencies">Dependencies</a>

* [R7.DotNetNuke.Extensions](https://github.com/roman-yagodin/R7.DotNetNuke.Extensions) (base library)
* [R7.ImageHandler](https://github.com/roman-yagodin/R7.ImageHandler) (automatic image thumbnailer)

## <a name="discuss">Setup discussion</a>

In order to setup discussions for *R7.News*, you need do the following:

1. Install (or ensure you have installed) latest [DNN Forum](https://github.com/juvander/DotNetNuke-Forum)
   or [ActiveForums](https://github.com/ActiveForums/ActiveForums) extensions.
2. Open `R7.News.yml` config file in portal root directory in text editor.
3. Set proper values for `params` for required provider in `discuss-provider` section.
   E.g. if you have some DNN Forum module instance (moduleId=145) placed on page with tabId=40
   and you want discussion posts to be created on specific forum (forumId=2), then your configuration
   should look like this:
   ```YAML
   discuss-providers:
     - type: R7.News.Providers.DiscussProviders.DnnForumDiscussProvider
       provider-key: DiscussOnDnnForum
       params: ['40', '145', '2'] # tabId, moduleId, forumId
    ```
4. Comment unused providers using `#` sign.
5. Restart application to apply changes.

To disable discussions, comment all lines in `discuss-providers` section of `R7.News.yml` config file, leaving only `discuss-providers:` line itself.

Note that you could develop and register your own discuss providers by implementing `IDiscussProvider` public interface.

To allow *R7.News* to use custom discuss provider:

1. Place a DLL with custom discuss provider class into `bin` folder of DNN install.
2. Register custom discuss provider using portal config file, adding assembly name followed by `:` to class name, like this:
   ```YAML
   discuss-providers:
     - type: MyCompany.DiscussProviders:MyCompany.DiscussProviders.MyCustomDiscussProvider
       provider-key: DiscussOnSomething
       params: ['your', 'custom', 'provider', 'params', 'here']
    ```
3. Restart application to apply changes.
4. If all OK, you'll be able to create discussions for news entries using new provider.
   If not, see DNN event log for more info about what's wrong.

## Uninstall

1. Uninstall *R7.News* library package first, this will remove all database objects and data.
2. Then uninstall *R7.News.Agent* and *R7.News.Stream* module packages (in any order).
3. Uninstall *R7.ImageHandler* and *R7.DotNetNuke.Extensions* library packages, if you don't need them.
