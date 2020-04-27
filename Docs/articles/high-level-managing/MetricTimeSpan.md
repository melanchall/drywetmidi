# MetricTimeSpan

[MetricTimeSpan](xref:Melanchall.DryWetMidi.Interaction.MetricTimeSpan) represents time span as a number of microseconds.

## Parsing

Following strings can be parsed to `MetricTimeSpan`:

`Hours : Minutes : Seconds : Milliseconds`  
`HoursGroup : Minutes : Seconds`  
`Minutes : Seconds`  
`Hours h Minutes m Seconds s Milliseconds ms`  
`Hours h Minutes m Seconds s`  
`Hours h Minutes m Milliseconds ms`  
`Hours h Seconds s Milliseconds ms`  
`Minutes m Seconds s Milliseconds ms`  
`Hours h Minutes m`  
`Hours h Seconds s`  
`Hours h Milliseconds ms`  
`Minutes m Seconds s`  
`Hours h Milliseconds ms`  
`Seconds s Milliseconds ms`  
`Hours h`  
`Minutes m`  
`Seconds s`  
`Milliseconds ms`

where

* **Hours** is a number of hours.
* **Minutes** is a number of minutes.
* **Seconds** is a number of seconds.
* **Milliseconds** is a number of milliseconds.

Examples:

`0:0:0:0` – zero time span  
`0:0:0:156` – 156 milliseconds  
`2:0:156` – 2 hours and 156 seconds  
`1:156` – 1 minute and 156 seconds  
`1h2m3s4ms` – 1 hour 2 minutes 3 seconds 4 milliseconds  
`1h 2m3s` – 1 hour 2 minutes 3 seconds  
`1h2M 4ms` – 1 hour 2 minutes 4 milliseconds  
`1 h3s4ms` – 1 hour 3 seconds 4 milliseconds  
`2M3 S 4 MS` – 2 minutes 3 seconds 4 milliseconds  
`1h2m` – 1 hour 2 minutes  
`1h 3s` – 1 hour 3 seconds  
`1h4MS` – 1 hour 4 milliseconds  
`2M3s` – 2 minutes 3 seconds  
`2 m 4 Ms` – 2 minutes 4 milliseconds  
`3 s 4 mS` – 2 seconds 4 milliseconds