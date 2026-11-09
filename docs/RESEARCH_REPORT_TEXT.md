# Smart-X dashboard engagement research

## Five strategies considered

1. Live trends with anomaly markers: reveal departures from normal sensor behaviour without requiring operators to scan raw packets (Iancu, Ciolofan & Drăgoicea, 2024).
2. Actionable alerts: identify the affected device and provide a direct route to investigation (Harum, Emran, Md Fauadi, Hamid, Khambari, Ridzuan & Kchouri, 2024).
3. Contextual drill-down: move from an overview to a sensor’s location, timestamps and history (Iancu et al., 2024).
4. Historical comparison: inspect earlier readings to distinguish isolated events from recurring patterns (Iancu et al., 2024).
5. Guided configuration feedback: give immediate field-specific validation and connection feedback. This is a proposed Smart-X adaptation of the usability concern identified by Harum et al. (2024).

## Chosen strategy and justification (500 words)

Smart-X adopts a live anomaly investigation dashboard as its primary engagement strategy. The objective is to help an operator move from noticing an unusual signal to identifying the affected device and recording a useful next action. Iancu, Ciolofan and Drăgoicea (2024) describe an IoT monitoring architecture that combines sensor visualisation, anomaly information and historical inspection. Their work supports connecting overview information with device context; Smart-X adapts that principle to a simulated greenhouse rather than reproducing their water monitoring implementation.

The operations screen therefore places recent telemetry beside a short attention list. Temperature spikes receive amber markers, while a device that stops reporting receives a separate disconnected label. This distinction matters: an unchanged value can be a legitimate measurement, whereas silence concerns communication. Smart-X calculates disconnection from server receipt time and the configured reporting interval. The operator can see both the expected cadence and the last received time on the sensor profile, reducing the need to infer connectivity from a chart alone.

A useful alert must also support investigation. Each event identifies the sensor, states whether the problem is a spike or a disconnect, and opens the relevant profile. The profile brings together recent values, deployment location, configuration attachments and an issue form. Harum, Emran, Md Fauadi, Hamid, Khambari, Ridzuan and Kchouri (2024) combine industrial data logging, dashboard visualisation and anomaly alerts. This provides a relevant precedent for pairing monitoring with actionable notifications. It does not establish that Smart-X achieves their usability results; that requires evaluation with its own users.

The visual design uses a restrained dark palette, consistent typography and clear separation between operational information and configuration forms. Status text accompanies colour so that meaning does not depend on colour discrimination. Only the selected signal is plotted, with explicit units and timestamps; temperature, wattage and valve states are never presented as interchangeable measurements. The history screen supplies exact packet values when a visual pattern needs confirmation. Pausing the view supports careful reading without stopping ingestion, while stopping the simulation is a separate control whose effect is explicitly described.

Implementation also limits distracting repetition. Repeated alerts for the same sensor and event kind are throttled, and operators can acknowledge an event without deleting its evidence. A bounded attention list keeps the overview readable across a large seeded fleet. Search and device profiles support further inspection when an operator needs a specific sensor. Configuration errors are returned as actionable messages, while upload completion and API availability receive visible feedback. These choices aim to maintain confidence during technical tasks instead of rewarding activity with decorative progress indicators.

The strategy should be evaluated through scripted spike, dropout and normal-operation scenarios. Record whether participants identify the correct device, choose an appropriate investigation step and successfully document the issue. Measure elapsed task time and incorrect diagnoses alongside brief usability feedback. Compare results with a plain telemetry table under the same scenarios. Until that comparison is completed, faster troubleshooting remains a design objective, not a demonstrated outcome. The same tasks should also be checked on narrow screens.

## References

Harum, N., Emran, N.A., Md Fauadi, M.H.F., Hamid, E., Khambari, M.N.M., Ridzuan, M.M. and Kchouri, M. 2024. An automated data logger system for real-time monitoring and anomaly detection in industrial IoT environment. Journal of Advanced Manufacturing Technology, 18(3):219–236. [Online]. Available at: https://jamt.utem.edu.my/jamt/article/view/6798 [Accessed 14 September 2026].

Iancu, G., Ciolofan, S.N. and Drăgoicea, M. 2024. Real-time IoT architecture for water management in smart cities. Discover Applied Sciences, 6:191. [Online]. Available at: https://doi.org/10.1007/s42452-024-05855-9 [Accessed 14 September 2026].