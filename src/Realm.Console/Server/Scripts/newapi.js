const a = createEntity("foo")
Logger.information("foo comp {f}", host.typeOf(TestComponent));
a.addComponent(new TestComponent("test comp name"))
const b = a.getComponent(host.typeOf(TestComponent))
Logger.information("comp b={b}", b);

