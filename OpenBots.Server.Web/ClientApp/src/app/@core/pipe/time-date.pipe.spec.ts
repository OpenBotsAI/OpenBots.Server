import { TimeDatePipe } from './time-date.pipe';

describe('TimeDatePipe', () => {
  it('create an instance', () => {
    const pipe = new TimeDatePipe();
    expect(pipe).toBeTruthy();
  });
});
