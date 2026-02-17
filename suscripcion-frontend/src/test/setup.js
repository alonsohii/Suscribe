jest.mock('axios');

const mockAxiosInstance = {
  post: jest.fn(),
  get: jest.fn(),
  interceptors: {
    response: {
      use: jest.fn((successHandler, errorHandler) => {
        mockAxiosInstance._successHandler = successHandler;
        mockAxiosInstance._errorHandler = errorHandler;
        return 0;
      }),
    },
  },
  _successHandler: null,
  _errorHandler: null,
};

const axios = require('axios');
axios.create.mockReturnValue(mockAxiosInstance);

global.mockAxiosInstance = mockAxiosInstance;
