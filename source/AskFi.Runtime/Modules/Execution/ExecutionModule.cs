using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Execution;
public class ExecutionModule
{
    private readonly BrokerMultiplexer _brokerMultiplexer;
    private readonly IPlatformPersistence _persistence;
    private readonly ChannelReader<NewDecision> _input;
    private readonly Channel<ActionExecuted> _output;

    public ChannelReader<ActionExecuted> Output => _output.Reader;

    public ExecutionModule(
        IReadOnlyDictionary<Type, object> brokers,
        IPlatformPersistence persistence,
        ChannelReader<NewDecision> input)
    {
        _brokerMultiplexer = new BrokerMultiplexer(brokers, persistence);
        _persistence = persistence;
        _input = input;
        _output = Channel.CreateUnbounded<ActionExecuted>();
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var executionSequence = ExecutionSequenceHead.Start;
        var executionSequenceCid = await _persistence.Put(executionSequence);

        await foreach (var decision in _input.ReadAllAsync(cancellationToken)) {
            var executionTasks = new List<Task<ActionExecutionResult>>();
            var executionInitiationMapping = new Dictionary<Task<ActionExecutionResult>, ActionInitiation>();

            var decisionHead = await _persistence.Get<DecisionSequenceHead>(decision.DecisionSequenceHeadCid);
            var decisionNode = decisionHead as DecisionSequenceHead.Initiative;
            var actionSet = await _persistence.Get<ActionSet>(decisionNode.Node.ActionSet);

            // Assign all action initiations an id and send to according broker instance
            foreach (var initiation in actionSet.Initiations) {
                if (_brokerMultiplexer.TryStartActionExecution(initiation, out var actionExecution)) {
                    // Broker available
                    executionTasks.Add(actionExecution);
                    executionInitiationMapping.Add(actionExecution, initiation);
                } else {
                    // No matching IBroker instance available. Do nothing.
                }
            }

            // Wait for all individual action initiations to complete.
            // Then immediately build the execution sequence.
            while (executionTasks.Count > 0) {
                var completed = await Task.WhenAny(executionTasks);
                var initiation = executionInitiationMapping[completed];
                executionTasks.Remove(completed);

                // Write and publish execution sequence
                executionSequence = ExecutionSequenceHead.NewExecution(new ExecutionSequenceNode(
                    previous: executionSequenceCid,
                    action: initiation));

                executionSequenceCid = await _persistence.Put(decisionSequence);

                await _output.Writer.WriteAsync(new ActionExecuted(executionSequenceCid));
            }
        }
    }
}
