"""
craft_api.py -- The pycraft API

"""


import msgpack
import zmq


class CraftAPI:
    """a class for pycraft API"""
    def __init__(self):
        """
        The constructor for class CraftAPI

        The constructor creates ZeroMQ context and connects the socket to
        port 1234, the default port of pycraft.
        """
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.PUSH)
        self.socket.connect('tcp://localhost:1234')


    def add_block(self, position, texture):
        """
        add_block -- add a block with specified texture to a given position

        Argument:
        position -- the position of the block
        texture -- the texture of the block
        """
        command = {'cmd': 'add_block',
                   'args': {'position': position, 'texture': texture}}
        message = msgpack.packb(command, use_bin_type=True)
        self.socket.send(message)


    def delete_block(self, position):
        """
        delete_block -- delete a block in a given position

        Arguments:
        position -- the position of the block
        """
        command = {'cmd': 'delete_block',
                   'args': {'position': position}}
        message = msgpack.packb(command, use_bin_type=True)
        self.socket.send(message)
