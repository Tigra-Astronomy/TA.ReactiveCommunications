using System.IO.Ports;
using Machine.Specifications;

namespace TA.Ascom.ReactiveCommunications.Specifications
    {
    [Subject(typeof(DeviceEndpoint))]
    public class when_creating_a_serial_device_endpoint_from_a_fully_specified_connection_string
        {
        Because of = () => Endpoint = DeviceEndpoint.FromConnectionString("COM127:115200,Even,7,OnePointFive,NOdtr,norts");
        It should_be_a_serial_endpoint = () => Endpoint.ShouldBeOfExactType<SerialDeviceEndpoint>();
        It should_target_com127 = () => ((SerialDeviceEndpoint)Endpoint).PortName.ShouldEqual("COM127");
        It should_have_baud_rate = () => ((SerialDeviceEndpoint)Endpoint).BaudRate.ShouldEqual(115200);
        It should_have_parity = () => ((SerialDeviceEndpoint)Endpoint).Parity.ShouldEqual(Parity.Even);
        It should_have_data_bits = () => ((SerialDeviceEndpoint)Endpoint).DataBits.ShouldEqual(7);
        It should_have_stop_bits = () => ((SerialDeviceEndpoint)Endpoint).StopBits.ShouldEqual(StopBits.OnePointFive);
        It should_not_assert_dtr = () => ((SerialDeviceEndpoint)Endpoint).DtrEnable.ShouldBeFalse();
        It should_not_assert_rts = () => ((SerialDeviceEndpoint)Endpoint).RtsEnable.ShouldBeFalse();
        static DeviceEndpoint Endpoint;
        }

    [Subject(typeof(DeviceEndpoint), "network")]
    public class when_creating_an_endpoint_from_a_lan_connection_string
        {
        Because of = () => Endpoint = DeviceEndpoint.FromConnectionString("localhost:1234");
        It should_be_a_network_endpoint = () => Endpoint.ShouldBeOfExactType<NetworkDeviceEndpoint>();
        It should_be_localhost = () => ((NetworkDeviceEndpoint)Endpoint).Host.ShouldEqual("localhost");
        It should_have_the_expected_port = () => ((NetworkDeviceEndpoint)Endpoint).Port.ShouldEqual(1234);
        static DeviceEndpoint Endpoint;
        }

    [Subject(typeof(DeviceEndpoint), "network")]
    public class when_creating_an_endpoint_from_a_wan_connection_string
        {
        Because of = () => Endpoint = DeviceEndpoint.FromConnectionString("www.tigranetworks.co.uk:1234");
        It should_be_a_network_endpoint = () => Endpoint.ShouldBeOfExactType<NetworkDeviceEndpoint>();
        It should_be_localhost = () => ((NetworkDeviceEndpoint)Endpoint).Host.ShouldEqual("www.tigranetworks.co.uk");
        It should_have_the_expected_port = () => ((NetworkDeviceEndpoint)Endpoint).Port.ShouldEqual(1234);
        static DeviceEndpoint Endpoint;
        }

    [Subject(typeof(DeviceEndpoint), "network")]
    public class when_creating_an_endpoint_from_an_ipv4_connection_string
        {
        Because of = () => Endpoint = DeviceEndpoint.FromConnectionString("1.12.123.123:1234");
        It should_be_a_network_endpoint = () => Endpoint.ShouldBeOfExactType<NetworkDeviceEndpoint>();
        It should_be_localhost = () => ((NetworkDeviceEndpoint)Endpoint).Host.ShouldEqual("1.12.123.123");
        It should_have_the_expected_port = () => ((NetworkDeviceEndpoint)Endpoint).Port.ShouldEqual(1234);
        static DeviceEndpoint Endpoint;
        }

    }