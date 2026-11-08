// These hooks affect only configurations explicitly enabled by the Windows integration tests.

struct WmsTestGroupTerminalBlock :
    winrt::implements<WmsTestGroupTerminalBlock, midi2::Enumeration::IMidiGroupTerminalBlock>
{
    WmsTestGroupTerminalBlock(uint8_t number, uint8_t firstGroup, uint8_t groupCount)
        : number(number), firstGroup(firstGroup), groupCount(groupCount)
    {
    }

    uint8_t Number() const { return number; }

    winrt::hstring Name() const { return L"WMS test block"; }

    midi2::Enumeration::MidiGroupTerminalBlockDirection Direction() const
    {
        return midi2::Enumeration::MidiGroupTerminalBlockDirection::Bidirectional;
    }

    midi2::Enumeration::MidiGroupTerminalBlockProtocol Protocol() const
    {
        return midi2::Enumeration::MidiGroupTerminalBlockProtocol::Unknown;
    }

    midi2::MidiGroup FirstGroup() const { return midi2::MidiGroup(firstGroup); }

    uint8_t GroupCount() const { return groupCount; }

    bool IncludesGroup(const midi2::MidiGroup& group) const
    {
        return group.Index() >= firstGroup && group.Index() < firstGroup + groupCount;
    }

    uint16_t MaxDeviceInputBandwidthIn4KBitsPerSecondUnits() const { return 0; }

    uint16_t MaxDeviceOutputBandwidthIn4KBitsPerSecondUnits() const { return 0; }

    uint32_t CalculatedMaxDeviceInputBandwidthBitsPerSecond() const { return 0; }

    uint32_t CalculatedMaxDeviceOutputBandwidthBitsPerSecond() const { return 0; }

    midi2::Enumeration::MidiFunctionBlock AsEquivalentFunctionBlock() const { return nullptr; }

    uint8_t number;
    uint8_t firstGroup;
    uint8_t groupCount;
};

enum class WmsTestCounter
{
    LiveInputHandles,
    LiveOutputHandles,
    LiveEndpointInfos,
    PluginsPresentBeforeOpen,
    PendingLoopbackRemovals
};

enum class WmsTestSessionCounter
{
    CachedConnections,
    CachedEndpointInfos,
    AttachedPlugins
};

API_EXPORT void API_CALL EnableWmsTests_Win(Configuration* configuration)
{
    configuration->testState = std::make_shared<WmsTestState>();
}

API_EXPORT void API_CALL SetWmsTestFailure_Win(Configuration* configuration, WmsTestFailure failure, int enabled)
{
    auto state = configuration->testState;

    switch (failure)
    {
        case WmsTestFailure::OpenConnection: state->failOpenConnection = enabled != 0; break;
        case WmsTestFailure::AddPlugin: state->failAddPlugin = enabled != 0; break;
        case WmsTestFailure::AfterLoopbackCreation: state->failAfterLoopbackCreation = enabled != 0; break;
        case WmsTestFailure::RemoveLoopback: state->failRemoveLoopback = enabled != 0; break;
    }
}

API_EXPORT void API_CALL SetWmsTestProtocol_Win(Configuration* configuration, WmsTestProtocol protocol)
{
    configuration->testState->protocol = protocol;
}

API_EXPORT int API_CALL GetWmsTestCounter_Win(Configuration* configuration, WmsTestCounter counter)
{
    auto state = configuration->testState;

    switch (counter)
    {
        case WmsTestCounter::LiveInputHandles: return state->liveInputHandles;
        case WmsTestCounter::LiveOutputHandles: return state->liveOutputHandles;
        case WmsTestCounter::LiveEndpointInfos: return state->liveEndpointInfos;
        case WmsTestCounter::PluginsPresentBeforeOpen: return state->pluginsPresentBeforeOpen;
        case WmsTestCounter::PendingLoopbackRemovals:
        {
            std::lock_guard<std::mutex> lock(configuration->pendingLoopbacksLock);
            return static_cast<int>(configuration->pendingLoopbacks.size());
        }
        default: throw winrt::hresult_invalid_argument();
    }
}

API_EXPORT int API_CALL GetWmsTestSessionCounter_Win(SessionHandle* sessionHandle, WmsTestSessionCounter counter)
{
    std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

    switch (counter)
    {
        case WmsTestSessionCounter::CachedConnections: return static_cast<int>(sessionHandle->endpointConnectionsById.Size());
        case WmsTestSessionCounter::CachedEndpointInfos:
        {
            size_t count = 0;

            for (const auto& pair : sessionHandle->endpointDevicesById)
            {
                count += pair.second.inputEndpointsInfo.size() + pair.second.outputEndpointsInfo.size();
            }

            return static_cast<int>(count);
        }
        case WmsTestSessionCounter::AttachedPlugins:
        {
            uint32_t count = 0;

            for (const auto& pair : sessionHandle->endpointConnectionsById)
            {
                count += pair.Value().MessageProcessingPlugins().Size();
            }

            return static_cast<int>(count);
        }
        default: throw winrt::hresult_invalid_argument();
    }
}

API_EXPORT void API_CALL ProcessWmsTestEndpoint_Win(
    SessionHandle* sessionHandle,
    VirtualDeviceInfo* virtualDeviceInfo,
    int isAdded)
{
    auto endpointId = winrt::hstring{ virtualDeviceInfo->endpointDeviceId };
    auto endpointInformation = midi2::Enumeration::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);

    if (isAdded && SupportsMidi1(sessionHandle->configuration->testState, endpointInformation))
        sessionHandle->processEndpointAdded(endpointInformation);
    else
        sessionHandle->processEndpointRemoved(endpointId);
}

API_EXPORT void API_CALL SetWmsTestEnumerationCompleted_Win(SessionHandle* sessionHandle, int completed)
{
    sessionHandle->initialEnumerationCompleted = completed != 0;
}

API_EXPORT void* API_CALL CreateWmsTestEndpointInfo_Win(
    Configuration* configuration,
    VirtualDeviceInfo* virtualDeviceInfo,
    int isInput,
    int blockNumber,
    int firstGroup,
    int groupCount,
    int groupIndex)
{
    auto groupTerminalBlock = winrt::make<WmsTestGroupTerminalBlock>(
        static_cast<uint8_t>(blockNumber),
        static_cast<uint8_t>(firstGroup),
        static_cast<uint8_t>(groupCount)).as<midi2::Enumeration::MidiGroupTerminalBlock>();
    auto group = midi2::MidiGroup(static_cast<uint8_t>(groupIndex));

    if (isInput)
    {
        InputEndpointInfo* info = nullptr;
        int errorCode;
        GetInputEndpointInfo(
            winrt::hstring{ virtualDeviceInfo->endpointDeviceId },
            groupTerminalBlock,
            group,
            &info,
            &errorCode);
        info->SetTestState(configuration->testState);
        return info;
    }

    OutputEndpointInfo* info = nullptr;
    int errorCode;
    GetOutputEndpointInfo(
        winrt::hstring{ virtualDeviceInfo->endpointDeviceId },
        groupTerminalBlock,
        group,
        &info,
        &errorCode);
    info->SetTestState(configuration->testState);
    return info;
}
