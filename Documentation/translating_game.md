# Translating Flock Around

This document will explain how to translate Flock Around into a new language. It will assume you know how to make and edit Configs in Flock Around.

## Localization Table Revisited
You can think of the Localization Data as a giant spreadsheet, or maybe even a database. Every row of the spreadsheet is a "String", some chunk of text that appears in game.

This spreadsheet looks like the following:

| id    |  slug   | English | French | German |
| --- | --- | --- | ---| --- |
| 2041791290    | main_menu.create_game    | Create Game | Créer une partie | Spiel erstellen |
| 1969315153    | main_menu.join_game    | Join Game | Rejoindre une partie | Spiel beitreten |
| ....    | ....           | .... |  .... | .... |
| ....    | ....           | .... |  .... | .... |
| ....    | ....           | .... |  .... | .... |

Most of the columns are Locales, but the first two are special.

The first column is the unique ID of the string. This is what every other part of the game keeps track of. The "Create Game" button doesn't know that it's the "Create Game" button, it just knows it's `2041791290`.


The second column is the "slug." The slug is the canonical name of the string, so we don't have to use the English string as its name. The English string is ambiguous "Back" could refer to a pose "Back of the bird" or it could refer to the user interface operation "Go back." So we might use the slugs like `guidebook.pose.back` and `ui.back` to disambiguate them.

The remaining columns are the Locale data, the main thing we're interested in talking about here. Every `Locale` Config in the game automatically adds its data to this table as a column. A `Locale` config is essentially just a collection of tuples that map the `id` column to the translation.

For instance, the English Locale data would look like this:

| id | English |
| --- | --- |
| 2041791290    | Create Game |
|  1969315153    | Join Game |

This is to say: Locales do not care about slugs, they only care about ids.

## Creating a Locale

Simple create a new Locale Config in the config tool.

- `New Config` > `Locale` > (name it whatever is appropriate)

Locales have some fields that need to be filled out in the config tool:

- `Locale Code` this is the (usually) 4 letter ISO locale code for this language in the format of `language_REGION`. You can look at the existing Locale configs for reference here.
- `Localized Name` is the native name of the language, aka: what the language calls itself. English is "English" but Brazilian Portoguese is "Português (Brasil)"
- `Sort Order` determines what order the locale will show up in the dropdown. The existing languages are numbered 0 through 8 so your locale should either be 9+ if you want it at the bottom of the dropdown, or -1 if you want it at the top of the dropdown. If two locales share a sort order the sort is nondeterministic.
- `Is Debug Only` should be turned off. Turning this on will prevent the language from showing up in the language selector.
- `Steam Name` is the language's name according to this list provided by Valve: https://partner.steamgames.com/doc/store/localization/languages. This is used in contexts where we interface with Steam's localization system (eg: uploading a Steam Workshop mod).

You'll notice that the actual "data" is missing from this list. That's because there is _a lot_ of data and rendering it all in the config tool would degrade performance. It's also probably preferable to edit it in an external tool anyway.

## Export CSV
Once your config is setup, you can run `loc export` in the console and it will generate a .csv file in your AppData directory, the output of the command should tell you where it ended up.

This `.csv` file should include all localized strings that the game knows about, and all locales the game knows about. This includes strings that were added via mods.

You can then import this `.csv` into Google Sheets, or Microsoft Excel, or whatever your preferred spreadsheet editing tool is. You can then fill in your translations. Please ensure the column names maintain their format, this will ensure we can re-import the `.csv` later.

## Actually doing the Translation
The next step is to painstakingly translate every single string in the game.... Have fun!

## Import CSV
Once you've finished (or you've progress and just want to see it in-game) you can run `loc import ` followed by the full path to your csv.

`NOTE!` The import only works on .csv files, if you imported the file into Excel and saved it as an `.xlsx`, you will need to "Save As" the file back down to a `.csv`.

You can then select your Locale in the Settings if you haven't done so already and you should see your translations in action! You may need to restart the game to fully reload the table.