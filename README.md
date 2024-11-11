
# Fußball.de REST API

This tool scrapes the Fußball.de page for some club and game data that can be accessed via a REST API.

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
[![.NET build and test](https://github.com/iste2/Fu-ball.de-REST-API/actions/workflows/dotnet.yml/badge.svg)](https://github.com/iste2/Fu-ball.de-REST-API/actions/workflows/dotnet.yml)
[![Docker Image CI](https://github.com/iste2/Fu-ball.de-REST-API/actions/workflows/docker-image.yml/badge.svg)](https://github.com/iste2/Fu-ball.de-REST-API/actions/workflows/docker-image.yml)

## Run Locally

Clone the project

```bash
  git clone https://github.com/iste2/Fu-ball.de-REST-API.git
```

Go to the project directory

```bash
  cd .\Fußball.de.Rest.Api\Fußball.de.Rest.Api
```

Run

```bash
  dotnet run
```



## API Reference

#### Get all teams of a club

```http
GET /teams/club/{id}/season/{season}
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `string` | **Required**. Id of the club |
| `season` | `string` | **Required**. The season ("2324" means season 2023-2024.) |

#### Get games of a team

```http
GET /games/team/{id}/start/{start}/end/{end}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `id`      | `string` | **Required**. Id of the team |
| `start`      | `string` | **Required**. Earliest date (Format: dd.MM.yyyy) |
| `end`      | `string` | **Required**. Latest date (Format: dd.MM.yyyy) |

A maximum of 100 games can be loaded in one call.


## Usage/Examples

### Get the teams of a club

First you have find out the id of your club. This can be done by visiting the club's page on Fußball.de.
In this example you find this page at https://www.fussball.de/verein/eintracht-kornelimuenster-mittelrhein/-/id/00ES8GN92C0000B1VV0AG08LVUPGND5I#!/. You can pull the id out of the link: "00ES8GN92C0000B1VV0AG08LVUPGND5I".

You are interesed in season 2024-2025.

So your API-call looks like:

```http
GET /teams/club/00ES8GN92C0000B1VV0AG08LVUPGND5I/season/2425
```

You get a response in form of a json list:

```json
[
  {
    "id": "011MIF5Q28000000VTVG0001VTR8C1K7",
    "name": "Herren - Eintracht Kornelimünster",
    "link": "https://www.fussball.de/mannschaft/eintracht-kornelimuenster-eintracht-kornelimuenster-mittelrhein/-/saison/2425/team-id/011MIF5Q28000000VTVG0001VTR8C1K7"
  },
  {
    "id": "011MIC82IO000000VTVG0001VTR8C1K7",
    "name": "Herren - Eintracht Kornelimünster II",
    "link": "https://www.fussball.de/mannschaft/eintracht-kornelimuenster-ii-eintracht-kornelimuenster-mittelrhein/-/saison/2425/team-id/011MIC82IO000000VTVG0001VTR8C1K7"
  },
  {
    "...": "..."
  }
]
```

### Get the games of a team

First you have find out the id of your team. This can be done by visiting the team's page on Fußball.de.
In this example you find this page at https://www.fussball.de/mannschaft/eintracht-kornelimuenster-eintracht-kornelimuenster-mittelrhein/-/saison/2425/team-id/011MIF5Q28000000VTVG0001VTR8C1K7#!/. You can pull the id out of the link: "011MIF5Q28000000VTVG0001VTR8C1K7".

You are interesed in the timespan 01.01.2024 - 30.06.2024.

So your API-call looks like:

```http
GET /games/team/011MIF5Q28000000VTVG0001VTR8C1K7/start/01.01.2024/end/30.06.2024
```

You get a response in form of a json list:

```json
[
  {
    "id": "02O7QCU7IK000000VS5489B3VURLU49U",
    "kickOff": "2024-01-28T11:00:00",
    "link": "https://www.fussball.de/spiel/eintracht-kornelimuenster-vfr-venwegen/-/spiel/02O7QCU7IK000000VS5489B3VURLU49U",
    "homeSide": {
      "id": "011MIF5Q28000000VTVG0001VTR8C1K7",
      "name": "Eintracht Kornelimünster",
      "link": "https://www.fussball.de/mannschaft/eintracht-kornelimuenster-eintracht-kornelimuenster-mittelrhein/-/saison/2324/team-id/011MIF5Q28000000VTVG0001VTR8C1K7"
    },
    "awaySide": {
      "id": "0132710U6S000000VV0AG80NVTPH626V",
      "name": "VfR Venwegen",
      "link": "https://www.fussball.de/mannschaft/vfr-venwegen-vfr-1920-ev-venwegen-mittelrhein/-/saison/2324/team-id/0132710U6S000000VV0AG80NVTPH626V"
    },
    "league": "Kreisfreundschaftsspiele",
    "squad": "Freundschaftsspiele Kreis Aachen",
    "squadId": "260007",
    "address": "Kunstrasenplatz, Sportplatz Inda-Gymnasium, Romerich, 52076 Aachen",
    "dfbnetId": "260007504",
    "goalsHome": "3",
    "goalsAway": "0",
    "goalsHomeHalf": "1",
    "goalsAwayHalf": "0"
  },
  {
    "id": "02O7QD6LRK000000VS5489B3VURLU49U",
    "kickOff": "2024-02-04T11:00:00",
    "link": "https://www.fussball.de/spiel/eintracht-kornelimuenster-sv-rott-ii/-/spiel/02O7QD6LRK000000VS5489B3VURLU49U",
    "homeSide": {
      "id": "011MIF5Q28000000VTVG0001VTR8C1K7",
      "name": "Eintracht Kornelimünster",
      "link": "https://www.fussball.de/mannschaft/eintracht-kornelimuenster-eintracht-kornelimuenster-mittelrhein/-/saison/2324/team-id/011MIF5Q28000000VTVG0001VTR8C1K7"
    },
    "awaySide": {
      "id": "02EO1HVAUG000000VS5489B2VSAS84KM",
      "name": "SV Rott II",
      "link": "https://www.fussball.de/mannschaft/sv-rott-ii-sv-rott-ev-mittelrhein/-/saison/2324/team-id/02EO1HVAUG000000VS5489B2VSAS84KM"
    },
    "league": "Kreisfreundschaftsspiele",
    "squad": "Freundschaftsspiele Kreis Aachen",
    "squadId": "260007",
    "address": "Kunstrasenplatz, Sportplatz Inda-Gymnasium, Romerich, 52076 Aachen",
    "dfbnetId": "260007505",
    "goalsHome": "1",
    "goalsAway": "3",
    "goalsHomeHalf": "0",
    "goalsAwayHalf": "0"
  },
  {
    "...": "..."
  }
]
```

### Get the games of a club

First you have find out the id of your club. This can be done by visiting the club's page on Fußball.de.
In this example you find this page at https://www.fussball.de/verein/eintracht-kornelimuenster-mittelrhein/-/id/00ES8GN92C0000B1VV0AG08LVUPGND5I#!/. You can pull the id out of the link: "00ES8GN92C0000B1VV0AG08LVUPGND5I".

You are interesed in the timespan 09.11.2024 - 09.11.2024, so specifically the games of this day.

So your API-call looks like:

```http
GET /games/club/00ES8GN92C0000B1VV0AG08LVUPGND5I/start/09.11.2024/end/09.11.2024
```

You get a response in form of a json list:

```json
[
  {
    "id": "02R12VKB28000000VS5489B4VTKFKS2T",
    "kickOff": "2024-11-09T18:15:00",
    "link": "https://www.fussball.de/spiel/eintracht-kornelimuenster-a1-djk-fv-haaren/-/spiel/02R12VKB28000000VS5489B4VTKFKS2T",
    "homeSide": {
      "id": "02PDPBI76S000000VS5489B1VVBNKSSO",
      "clubId": "00ES8GN92C0000B1VV0AG08LVUPGND5I",
      "name": "Eintracht Kornelimünster",
      "link": "https://www.fussball.de/mannschaft/eintracht-kornelimuenster-a1-eintracht-kornelimuenster-mittelrhein/-/saison/2425/team-id/02PDPBI76S000000VS5489B1VVBNKSSO",
      "logoUrl": "https://www.fussball.de/export.media/-/action/getLogo/format/12/id/00ES8GN92C0000B1VV0AG08LVUPGND5I",
      "kind": "A-Junioren"
    },
    "awaySide": {
      "id": "02PP6C5LNK000000VS5489B1VU7RM1AE",
      "clubId": "00ES8GN92C0000A9VV0AG08LVUPGND5I",
      "name": "DJK FV Haaren",
      "link": "https://www.fussball.de/mannschaft/djk-fv-haaren-djk-fv-haaren-mittelrhein/-/saison/2425/team-id/02PP6C5LNK000000VS5489B1VU7RM1AE",
      "logoUrl": "https://www.fussball.de/export.media/-/action/getLogo/format/12/id/00ES8GN92C0000A9VV0AG08LVUPGND5I",
      "kind": "A-Junioren"
    },
    "league": "Kreisleistungsklasse",
    "squad": "A-Jun., Leistungsstaffel",
    "squadId": "237011",
    "address": "Kunstrasenplatz, Sportplatz Inda-Gymnasium, Romerich, 52076 Aachen",
    "dfbnetId": "237011007",
    "goalsHome": "8",
    "goalsAway": "0",
    "goalsHomeHalf": "3",
    "goalsAwayHalf": "0"
  },
  {
    "id": "02Q797T174000000VS5489B4VTFDT7S8",
    "kickOff": "2024-11-09T17:15:00",
    "link": "https://www.fussball.de/spiel/sg-merkstein-ritzerfeld-eintracht-kornelimuenster-b1/-/spiel/02Q797T174000000VS5489B4VTFDT7S8",
    "homeSide": {
      "id": "02IKN0RK04000000VS5489B1VSO9MMK0",
      "clubId": "00ES8GN9A000000UVV0AG08LVUPGND5I",
      "name": "SG Merkstein/&#8203;Ritzerfeld",
      "link": "https://www.fussball.de/mannschaft/sg-merkstein-ritzerfeld-sv-conc-1927-ev-merkstein-mittelrhein/-/saison/2425/team-id/02IKN0RK04000000VS5489B1VSO9MMK0",
      "logoUrl": "https://www.fussball.de/export.media/-/action/getLogo/format/12/id/00ES8GN9A000000UVV0AG08LVUPGND5I",
      "kind": "B-Junioren"
    },
    "awaySide": {
      "id": "02B84V91SO000000VS5489B1VV9M5FHC",
      "clubId": "00ES8GN92C0000B1VV0AG08LVUPGND5I",
      "name": "Eintracht Kornelimünster",
      "link": "https://www.fussball.de/mannschaft/eintracht-kornelimuenster-b1-eintracht-kornelimuenster-mittelrhein/-/saison/2425/team-id/02B84V91SO000000VS5489B1VV9M5FHC",
      "logoUrl": "https://www.fussball.de/export.media/-/action/getLogo/format/12/id/00ES8GN92C0000B1VV0AG08LVUPGND5I",
      "kind": "B-Junioren"
    },
    "league": "Kreissonderliga",
    "squad": "Sonderliga B-Junioren",
    "squadId": "237002",
    "address": "Kunstrasenplatz, Sportplatz Marie-Juchacz-Strasse, Marie-Juchacz-Str., 52134 Herzogenrath",
    "dfbnetId": "237002048",
    "goalsHome": "3",
    "goalsAway": "6",
    "goalsHomeHalf": "1",
    "goalsAwayHalf": "4"
  },
  {
    "...": "..."
  }
]
```

## Roadmap

- Host this project to make the API generally available

