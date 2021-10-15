from bus import MessageDispatcher, messages as m
from events.meteringpoint import ConsumptionMeteringPointCreated, SettlementMethodUpdated

def on_consumption_metering_point_created(msg: ConsumptionMeteringPointCreated):
    print("create event with id "+msg.id)

def on_settlement_method_updated(msg: SettlementMethodUpdated):
    print("update smethod"+msg.settlement_method+" on id "+msg.metering_point_id)

# -- Dispatcher --------------------------------------------------------------


dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
