#pragma once

#include <numeric>
#include <vector>

class Stream
{
private:
	std::vector<int> _buffer;
	size_t _position;

public:
	Stream(size_t size) :
		_buffer(size),
		_position(0)
	{
		std::iota(_buffer.begin(), _buffer.end(), 0); 
	}

	size_t GetSize() const
	{
		return _buffer.size();
	}

	size_t GetPosition() const
	{
		return _position;
	}

	size_t Read(int* const buffer, size_t size)
	{
		auto items = min(size, _buffer.size() - _position);
		memcpy(buffer, &_buffer.at(_position), items * sizeof(int));
		_position += items;
		return items;
	}

	void Reset()
	{
		_position = 0;
	}
};



