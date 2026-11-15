![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

# foxco-lubelog

**[English](#english) · [Deutsch](#deutsch)**

Personal fork of [LubeLogger](https://github.com/hargata/lubelog) (project name `CarCareTracker`) — self-hosted, open-source vehicle maintenance and fuel mileage tracker.

Fork version: **1.8.4** (based on upstream **1.7.4**)

> Virtually all of the functionality and the code come from the upstream project by [Hargata](https://github.com/hargata). This fork only adds a few things for personal use. For documentation, support and anything fundamental, the original still applies.

> [!NOTE]
> This fork ships as a ready-to-run Linux build (`LubeLogger_<version>_linux_x64.zip`, .NET included) on the [Releases](https://github.com/FoxXxHater/foxco-lubelog/releases) page. There is **no Docker image**.

---

## English

### Changes compared to upstream

#### "Upcoming" overview

An additional tab in the garage overview that collects **every reminder and every open plan across all vehicles in an account** — instead of clicking through them vehicle by vehicle.

- Sorted by urgency, then by due date
- Filter by vehicle, urgency and free text; the counter badges update along with the filter
- Clicking a reminder opens the detail dialog, clicking a plan jumps to that vehicle's plan tab
- Direct link: `/?tab=upcoming`

#### Reminders in the past

The reminder date picker was restricted to future dates on the client side. That restriction is gone — reminders can be created with a date in the past and then show up as *Past Due* right away. The calendar tab now also displays past days.

#### Mark as done

Reminders can be checked off instead of only deleted:

- **Recurring** ones move on to their next interval, as before
- **Everything else** is archived rather than deleted — it disappears from the bell, the calendar, the kiosk, the upcoming overview and all counters, but stays in the vehicle's reminder tab struck through and can be reopened

#### Set urgency manually

The urgency level can be set by hand per reminder. It replaces the level calculated from date and odometer entirely — useful for things that are important or unimportant regardless of a due date. Overridden entries are marked with a pin icon in the lists.

#### Past Due is the loudest level

Upstream renders *Past Due* in grey even though it is internally the highest urgency level — an overdue entry therefore looked less alarming than one that was merely due soon. The order is now consistent: green → yellow → red → dark red (past due).

#### Translations from this fork

*Get Translations* pulls the language files from [foxco-lubelog_translations](https://github.com/FoxXxHater/foxco-lubelog_translations) instead of the upstream repo, so this fork's own strings ship with it.

Everything else — configuration, reverse proxy, Postgres and so on — is unchanged and covered by the [original documentation](https://docs.lubelogger.com/).

### Installation & update (Proxmox LXC)

The [community-scripts LubeLogger script](https://community-scripts.github.io/ProxmoxVE/scripts?id=lubelogger) installs the **upstream** release from `hargata/lubelog`, not this fork.

> [!WARNING]
> Running `update` inside the container installs the upstream version again. Your data is kept, but the features of this fork are gone.

To switch a container to this fork, or to update it, run this inside the container (as root) and set `VERSION` to the release you want:

```bash
VERSION=v1.8.4
systemctl stop lubelogger &&
mkdir -p /root/lubelogger-backup &&
cp -a /opt/lubelogger/data /opt/lubelogger/appsettings.json /root/lubelogger-backup/ &&
curl -fL -o /tmp/lubelogger.zip "https://github.com/FoxXxHater/foxco-lubelog/releases/download/${VERSION}/LubeLogger_${VERSION}_linux_x64.zip" &&
rm -rf /opt/lubelogger && mkdir -p /opt/lubelogger &&
unzip -q /tmp/lubelogger.zip -d /opt/lubelogger &&
cp -a /root/lubelogger-backup/. /opt/lubelogger/ &&
chmod 700 /opt/lubelogger/CarCareTracker &&
systemctl start lubelogger
```

The backup in `/root/lubelogger-backup` stays in place, so you can delete it once everything works. `appsettings.json` is restored as well, so the port set by the script (5000) is kept.

### Maintenance

- **Release:** push a tag like `v1.8.4`. [release.yml](.github/workflows/release.yml) builds the Linux zip and publishes the release.
- **Build check:** [build-check.yml](.github/workflows/build-check.yml) builds every pull request and every push to `main`.
- **Syncing with upstream:** merge `hargata/lubelog` `main` into a branch (merge, not rebase) and open a pull request. Docker files were removed in this fork, so if upstream changes them, resolve the conflict by deleting them again.

---

## Deutsch

### Änderungen gegenüber Upstream

#### Übersicht "Anstehende Termine"

Ein zusätzlicher Tab in der Garage-Übersicht, der **alle Erinnerungen und alle offenen Pläne über sämtliche Fahrzeuge eines Accounts** sammelt — statt sie pro Fahrzeug einzeln durchzuklicken.

- Sortiert nach Dringlichkeit, dann nach Fälligkeitsdatum
- Filter nach Fahrzeug, Dringlichkeit und Freitext; die Zähler-Badges rechnen live mit
- Klick auf eine Erinnerung öffnet den Detail-Dialog, Klick auf einen Plan springt zum Plan-Tab des Fahrzeugs
- Direktlink: `/?tab=upcoming`

#### Erinnerungen in der Vergangenheit

Der Datumsdialog von Erinnerungen war clientseitig auf zukünftige Daten beschränkt. Diese Sperre ist aufgehoben — Erinnerungen lassen sich mit einem Datum in der Vergangenheit anlegen und erscheinen dann direkt als *Überfällig*. Der Kalender-Tab zeigt jetzt ebenfalls vergangene Tage an.

#### Erledigt markieren

Erinnerungen lassen sich abhaken, nicht nur löschen:

- **Wiederkehrende** springen wie gehabt auf das nächste Intervall
- **Alle anderen** werden archiviert statt gelöscht — sie verschwinden aus Glocke, Kalender, Kiosk, Terminübersicht und allen Zählern, bleiben aber im Reminder-Tab des Fahrzeugs durchgestrichen erhalten und lassen sich wieder öffnen

#### Dringlichkeit manuell festlegen

Pro Erinnerung lässt sich die Dringlichkeitsstufe von Hand setzen. Sie ersetzt die aus Datum und Kilometerstand berechnete Stufe vollständig — nützlich für Dinge, die unabhängig von einem Termin wichtig oder unwichtig sind. Übersteuerte Einträge sind in den Listen mit einem Pin-Symbol gekennzeichnet.

#### Überfällig ist die auffälligste Stufe

Im Original wird *Überfällig* grau dargestellt, obwohl es intern die höchste Dringlichkeitsstufe ist — ein überfälliger Eintrag sah damit harmloser aus als ein bald fälliger. Die Reihenfolge ist jetzt durchgängig: grün → gelb → rot → dunkelrot (überfällig).

#### Übersetzungen aus dem eigenen Fork

*Get Translations* lädt die Sprachdateien aus [foxco-lubelog_translations](https://github.com/FoxXxHater/foxco-lubelog_translations) statt aus dem Upstream-Repo, damit die Begriffe dieses Forks mit ausgeliefert werden können.

Alles Weitere zu Konfiguration, Reverse Proxy, Postgres und Co. steht unverändert in der [Original-Dokumentation](https://docs.lubelogger.com/).

### Installation & Update (Proxmox LXC)

Das [LubeLogger-Skript der community-scripts](https://community-scripts.github.io/ProxmoxVE/scripts?id=lubelogger) installiert das Release des **Originals** aus `hargata/lubelog`, nicht diesen Fork.

> [!WARNING]
> `update` im Container spielt wieder die Originalversion ein. Die Daten bleiben erhalten, die Funktionen dieses Forks sind danach aber weg.

Um einen Container auf diesen Fork umzustellen oder ihn zu aktualisieren, im Container (als root) ausführen und `VERSION` auf das gewünschte Release setzen:

```bash
VERSION=v1.8.4
systemctl stop lubelogger &&
mkdir -p /root/lubelogger-backup &&
cp -a /opt/lubelogger/data /opt/lubelogger/appsettings.json /root/lubelogger-backup/ &&
curl -fL -o /tmp/lubelogger.zip "https://github.com/FoxXxHater/foxco-lubelog/releases/download/${VERSION}/LubeLogger_${VERSION}_linux_x64.zip" &&
rm -rf /opt/lubelogger && mkdir -p /opt/lubelogger &&
unzip -q /tmp/lubelogger.zip -d /opt/lubelogger &&
cp -a /root/lubelogger-backup/. /opt/lubelogger/ &&
chmod 700 /opt/lubelogger/CarCareTracker &&
systemctl start lubelogger
```

Die Sicherung in `/root/lubelogger-backup` bleibt liegen und kann gelöscht werden, wenn alles läuft. `appsettings.json` wird mit zurückgespielt, damit der vom Skript gesetzte Port (5000) erhalten bleibt.

### Wartung

- **Release:** Einen Tag wie `v1.8.4` pushen. [release.yml](.github/workflows/release.yml) baut das Linux-ZIP und veröffentlicht das Release.
- **Build-Check:** [build-check.yml](.github/workflows/build-check.yml) baut jeden Pull Request und jeden Push auf `main`.
- **Abgleich mit dem Original:** `main` von `hargata/lubelog` in einen Branch mergen (Merge, kein Rebase) und einen Pull Request öffnen. Die Docker-Dateien sind in diesem Fork gelöscht. Ändert das Original sie, den Konflikt lösen, indem man sie wieder löscht.

---

## Upstream

- Website: https://lubelogger.com
- [Documentation / Dokumentation](https://docs.lubelogger.com/)
- [Troubleshooting](https://docs.lubelogger.com/Installation/Troubleshooting)
- [Screenshots](/docs/screenshots.md)
- [Live Demo](https://demo.lubelogger.com) — user `test`, password `1234`
- [Issues in the original project](https://github.com/hargata/lubelog/issues)
- [Funding](https://docs.lubelogger.com/Misc/Funding) — supports the upstream project, not this fork

## Dependencies

- [Bootstrap](https://github.com/twbs/bootstrap)
- [LiteDB](https://github.com/mbdavid/litedb)
- [Npgsql](https://github.com/npgsql/npgsql)
- [Bootstrap-DatePicker](https://github.com/uxsolutions/bootstrap-datepicker)
- [SweetAlert2](https://github.com/sweetalert2/sweetalert2)
- [CsvHelper](https://github.com/JoshClose/CsvHelper)
- [Chart.js](https://github.com/chartjs/Chart.js)
- [Drawdown](https://github.com/adamvleggett/drawdown)
- [MailKit](https://github.com/jstedfast/MailKit)
- [Masonry](https://github.com/desandro/masonry)
- [QRCode-Generator](https://github.com/kazuhikoarase/qrcode-generator)

## License

MIT — same as the upstream project. Copyright of the original work belongs to Hargata.
