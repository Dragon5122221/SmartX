namespace SmartX.Shared.Contracts.Simulation;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record StartSimulationRequest(SmartX.Shared.Simulation.SimulationScenario Scenario = SmartX.Shared.Simulation.SimulationScenario.Mixed);
