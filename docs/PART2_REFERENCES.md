# Part 2 references and influence notes

IIE Harvard-style web references. `n.d.` is used where a stable publication date is not established; access dates are not substituted for publication dates. Part 1 references remain in `REFERENCES.md`.

Carbon Design System. n.d. *Dashboards*. [Online]. Available at: https://v10.carbondesignsystem.com/data-visualization/dashboards/ [Accessed 16 October 2026].

Grafana Labs. n.d. *Grafana dashboard best practices*. [Online]. Available at: https://grafana.com/docs/grafana/latest/visualizations/dashboards/build-dashboards/best-practices/ [Accessed 16 October 2026].

Microsoft. n.d.a. *Dictionary<TKey,TValue> Class*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-10.0 [Accessed 16 October 2026].

Microsoft. n.d.b. *HashSet<T> Class*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-10.0 [Accessed 16 October 2026].

Microsoft. n.d.c. *PriorityQueue<TElement,TPriority> Class*. [Online]. Available at: https://learn.microsoft.com/dotnet/api/system.collections.generic.priorityqueue-2 [Accessed 16 October 2026].

Microsoft. n.d.d. *Queue<T> Class*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1?view=net-10.0 [Accessed 16 October 2026].

Microsoft. n.d.e. *SortedDictionary<TKey,TValue> Class*. [Online]. Available at: https://learn.microsoft.com/en-gb/dotnet/api/system.collections.generic.sorteddictionary-2?view=net-10.0 [Accessed 16 October 2026].

Microsoft. n.d.f. *Stack<T> Class*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.stack-1?view=net-10.0 [Accessed 16 October 2026].

## Application to this project

The design uses operator-led information order and a clear relationship between monitoring and investigation, informed by Grafana Labs (n.d.). Integrated panes and restrained chart presentation draw on Carbon Design System (n.d.), extending the supplied template's existing palette. No external visual component library or copied dashboard template was required.

The normal queue and command stack follow FIFO and LIFO semantics respectively (Microsoft, n.d.d; Microsoft, n.d.f). Critical work uses a heap with an explicit arrival-order tie-breaker rather than assuming equal priorities are FIFO (Microsoft, n.d.c). A dictionary indexes device identifiers, while the sorted dictionary indexes historical observations (Microsoft, n.d.a; Microsoft, n.d.e). The active-error set expresses unique membership and recovery (Microsoft, n.d.b). The contextual support-count algorithm and trend presentation are original application logic; these references are not evidence that a six-sample forecast is operationally reliable.
