Who-is-Canberra
===============

Govhack 2014 project - Who is Canberra

WiC - ancestry script.Rmd - when knit queries the ABS ITT api to pull down ancestry data and generate the data files required by the engine.

WiC - birth script.Rmd - when knit processes a .csv file exported from abs.stat.gov.au to generate the country of birth data files required by the engine. This script could be updated to use the abs.stat api but there was not enough time to adapt it to the peculiarities of the api output.

Subfolder - ancestry contains the output of WiC - ancestry script.Rmd loaded into the engine
subfolder - country of birth contains the output of WiC - birth script loaded into the engine and the raw.csv file processed by the script.