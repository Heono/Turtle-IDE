"""
API handler module

"""


from threading import Thread

import msgpack
import zmq


class APIHandlerThread(Thread):
    """APIHandler class"""
    def __init__(self, msg_queue):
        """
        __init__ method

        Arguments:
        msg_queue -- the queue to put messages which feeds to Window
        """
        Thread.__init__(self)
        self.msg_queue = msg_queue


    def run(self):
        """
        run() method -- handles API requests

        More specifically, it reads api 'calls' from zmq and feed it to
        msg_queue.
        The api 'calls' are simply byte arrays encoded with utf-8, for now.
        It will be changed to msgpack-compressed json objects.
        """
        context = zmq.Context()
        socket = context.socket(zmq.PULL)
        # TODO: allow users to customize port
        socket.bind('tcp://*:1234')
        print('api handler thread started')

        while True:
            try:
                message = socket.recv()
            except zmq.error.ZMQError:
                # FIXME: doesn't seem to be right
                continue

            command = msgpack.unpackb(message, raw=False)
            self.msg_queue.put_nowait(command)
