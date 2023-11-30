# Performance

## @initDuration

| Runtime | Min | Max | Avg | 50% | 75% | 90% | 99% |
| --- | ---:| ---:| ---:| ---:| ---:| ---:| ---:|
| .NET 8 w/ AOT  |  99 | 150 | 112 | 111 | 114 | 118 | 149 |
| .NET 8 w/o AOT | 503 | 650 | 591 | 591 | 599 | 612 | 633 |
| .NET 6 w/ RTR  | 497 | 656 | 539 | 535 | 545 | 558 | 579 |
| .NET 6 w/o RTR | 558 | 710 | 641 | 637 | 648 | 669 | 706 |
| .NET 8 w/ AOT  |  96 | 149 | 108 | 107 | 111 | 115 | 124 |

## @billedDuration

| Runtime | Min | Max | Avg | 50% | 75% | 90% | 99% |
| --- | ---:| ---:| ---:| ---:| ---:| ---:| ---:|
| .NET 8 w/ AOT  |  204 |  255 |  218 |  216 |  220 |  224 |  254 |
| .NET 8 w/o AOT | 1106 | 1318 | 1255 | 1257 | 1281 | 1300 | 1311 |
| .NET 6 w/ RTR  | 1114 | 1323 | 1192 | 1194 | 1213 | 1238 | 1269 |
| .NET 6 w/o RTR | 1716 | 2053 | 1897 | 1890 | 1924 | 1956 | 2034 |
| .NET 8 w/ AOT  |  201 |  255 |  213 |  213 |  217 |  221 |  229 |




  ```
  fields @timestamp, @billedDuration, @duration, @initDuration
| filter (@message like "REPORT" and @message like "Init Duration")
| stats min(@initDuration), max(@initDuration), avg(@initDuration), pct(@initDuration, 50), pct(@initDuration, 75), pct(@initDuration, 90), pct(@initDuration, 99), min(@billedDuration), max(@billedDuration), avg(@billedDuration), pct(@billedDuration, 50), pct(@billedDuration, 75), pct(@billedDuration, 90), pct(@billedDuration, 99)
| limit 2000
  ```

## .NET 8 AOT

start-time: 2023-11-29T22:44:29.338Z
end-time: 2023-11-29T22:46:54.002Z

---
| v | min(@initDuration) | max(@initDuration) | avg(@initDuration) | pct(@initDuration, 50) | pct(@initDuration, 75) | pct(@initDuration, 90) | pct(@initDuration, 99) | min(@billedDuration) | max(@billedDuration) | avg(@billedDuration) | pct(@billedDuration, 50) | pct(@billedDuration, 75) | pct(@billedDuration, 90) | pct(@billedDuration, 99) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| .NET 8 w/ AOT | 98.93 | 150.25 | 112.3409 | 110.78 | 114.41 | 117.56 | 148.96 | 204 | 255 | 218.01 | 216 | 220 | 224 | 254 |
---

## .NET 8 w/o AOT

start-time: 2023-11-29T22:52:38.641Z
end-time: 2023-11-29T22:54:33.521Z

---
| v | min(@initDuration) | max(@initDuration) | avg(@initDuration) | pct(@initDuration, 50) | pct(@initDuration, 75) | pct(@initDuration, 90) | pct(@initDuration, 99) | min(@billedDuration) | max(@billedDuration) | avg(@billedDuration) | pct(@billedDuration, 50) | pct(@billedDuration, 75) | pct(@billedDuration, 90) | pct(@billedDuration, 99) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| .NET 8 w/o AOT | 503.35 | 649.69 | 591.401 | 591.36 | 598.74 | 612 | 633.36 | 1106 | 1318 | 1255.42 | 1257 | 1281 | 1300 | 1311 |
---

## .NET 6 w/ RTR

start-time: 2023-11-29T22:56:15.637Z
end-time: 2023-11-29T22:58:59.447Z

---
| v | min(@initDuration) | max(@initDuration) | avg(@initDuration) | pct(@initDuration, 50) | pct(@initDuration, 75) | pct(@initDuration, 90) | pct(@initDuration, 99) | min(@billedDuration) | max(@billedDuration) | avg(@billedDuration) | pct(@billedDuration, 50) | pct(@billedDuration, 75) | pct(@billedDuration, 90) | pct(@billedDuration, 99) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| .NET 6 w/ RTR | 496.9 | 655.9 | 538.5219 | 535.317 | 545.035 | 558.2674 | 578.7208 | 1114 | 1323 | 1192.19 | 1194 | 1213 | 1238 | 1269 |
---

## .NET 6 w/o RTR

start-time: 2023-11-29T23:02:27.932Z
end-time: 2023-11-29T23:04:41.959Z

---
| v | min(@initDuration) | max(@initDuration) | avg(@initDuration) | pct(@initDuration, 50) | pct(@initDuration, 75) | pct(@initDuration, 90) | pct(@initDuration, 99) | min(@billedDuration) | max(@billedDuration) | avg(@billedDuration) | pct(@billedDuration, 50) | pct(@billedDuration, 75) | pct(@billedDuration, 90) | pct(@billedDuration, 99) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| .NET 6 w/o RTR | 558 | 710.38 | 640.9236 | 637.0016 | 647.9177 | 668.9757 | 706.0745 | 1716 | 2053 | 1897.42 | 1890 | 1924 | 1956 | 2034 |
---

## .NET 8 w/ AOT

start-time: 2023-11-29T23:07:00.240Z
end-time: 2023-11-29T23:09:20.649Z

---
| v | min(@initDuration) | max(@initDuration) | avg(@initDuration) | pct(@initDuration, 50) | pct(@initDuration, 75) | pct(@initDuration, 90) | pct(@initDuration, 99) | min(@billedDuration) | max(@billedDuration) | avg(@billedDuration) | pct(@billedDuration, 50) | pct(@billedDuration, 75) | pct(@billedDuration, 90) | pct(@billedDuration, 99) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| .NET 8 w/ AOT | 95.64 | 149.48 | 107.9442 | 107.38 | 111.05 | 115.14 | 124 | 201 | 255 | 213.3 | 213 | 217 | 221 | 229 |
---
