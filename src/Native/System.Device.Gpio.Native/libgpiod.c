// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <gpiod.h>

/**
 * @brief Close a GPIO chip handle and release all allocated resources.
 * @param chip The GPIO chip object.
 */
extern void CloseChip(struct gpiod_chip *chip) 
{
	gpiod_chip_close(chip);
}

/**
 * @brief Get the number of GPIO lines exposed by this chip.
 * @param chip The GPIO chip object.
 * @return Number of GPIO lines.
 */
extern unsigned int GetNumberOfLines(struct gpiod_chip *chip) 
{
	return gpiod_chip_num_lines(chip);
}

/**
 * @brief Get the handle to the GPIO line at given offset.
 * @param chip The GPIO chip object.
 * @param offset The offset of the GPIO line.
 * @return Pointer to the GPIO line handle or NULL if an error occured.
 */
extern struct gpiod_line *GetChipLineByOffset(struct gpiod_chip *chip, unsigned int offset)
{
	return gpiod_chip_get_line(chip, offset);
}

/**
 * @brief Read the GPIO line direction setting.
 * @param line GPIO line object.
 * @return Returns GPIOD_DIRECTION_INPUT or GPIOD_DIRECTION_OUTPUT.
 */
extern int GetLineDirection(struct gpiod_line *line) 
{
	return gpiod_line_direction(line);
}

/**
 * @brief Reserve a single line, set the direction to input.
 * @param line GPIO line object.
 * @param consumer Name of the consumer.
 * @return 0 if the line was properly reserved, -1 on failure.
 */
extern int RequestLineInput(struct gpiod_line *line, const char *consumer) 
{
	return gpiod_line_request_input(line, consumer);
}

/**
 * @brief Reserve a single line, set the direction to output.
 * @param line GPIO line object.
 * @param consumer Name of the consumer.
 * @return 0 if the line was properly reserved, -1 on failure.
 */
extern int RequestLineOutput(struct gpiod_line *line, const char *consumer) 
{
	return gpiod_line_request_output(line, consumer, 0);
}
		
/**
 * @brief Check if the calling user has neither requested ownership of this line nor configured any event notifications.
 * @param line GPIO line object.
 * @return True if given line is free, false otherwise.
 */		
extern bool LineIsFree(struct gpiod_line *line) 
{
	return gpiod_line_is_free(line);
}

		
/**
 * @brief Release a previously reserved line.
 * @param line GPIO line object.
 */
extern void ReleaseGpiodLine(struct gpiod_line *line) 
{
	gpiod_line_release(line);
}

/**
 * @brief Read current value of a single GPIO line.
 * @param line GPIO line object.
 * @return 0 or 1 if the operation succeeds. On error this routine returns -1
 *         and sets the last error number.
 */
extern int GetGpiodLineValue(struct gpiod_line *line) 
{
	return gpiod_line_get_value(line);
}

/**
 * @brief Set the value of a single GPIO line.
 * @param line GPIO line object.
 * @param value New value.
 * @return 0 is the operation succeeds. In case of an error this routine
 *         returns -1 and sets the last error number.
 */
extern int SetGpiodLineValue(struct gpiod_line *line, int value) 
{
	return gpiod_line_set_value(line, value);
}

/**
 * @brief Create a new gpiochip iterator.
 * @return Pointer to a new chip iterator object or NULL if an error occurred.
 */
extern struct gpiod_chip_iter * GetChipIterator(void) 
{
	return gpiod_chip_iter_new();
}

/**
 * @brief Release all resources allocated for the gpiochip iterator and close
 *        the most recently opened gpiochip (if any).
 * @param iter The gpiochip iterator object.
 */
extern void FreeChipIterator(struct gpiod_chip_iter *iter) 
{
	gpiod_chip_iter_free(iter);
}

/**
 * @brief Release all resources allocated for the gpiochip iterator but
 *        don't close the most recently opened gpiochip (if any).
 * @param iter The gpiochip iterator object.
 */
extern void FreeChipIteratorNoCloseCurrentChip(struct gpiod_chip_iter *iter) 
{
	gpiod_chip_iter_free_noclose(iter);
}

/**
 * @brief Get the next gpiochip handle.
 * @param iter The gpiochip iterator object.
 * @return Pointer to the next open gpiochip handle or NULL if no more chips
 *         are present in the system.
 * @note The previous chip handle will be closed.
 */
extern struct gpiod_chip * GetNextChipFromChipIterator(struct gpiod_chip_iter *iter) 
{
	return gpiod_chip_iter_next(iter); 
}

/**
 * @brief Request all event type notifications on a single line.
 * @param line GPIO line object.
 * @param consumer Name of the consumer.
 * @return 0 if the operation succeeds, -1 on failure.
 */
extern int RequestBothEdgesEventForLine(struct gpiod_line *line, const char *consumer) 
{
	return gpiod_line_request_both_edges_events(line, consumer);
}

/**
 * @brief Wait for an event on a single line.
 * @param line GPIO line object.
 * @return 0 if wait timed out, -1 if an error occurred, 1 if an event occurred.
 */
extern int WaitForEventOnLine(struct gpiod_line *line)
{
	struct timespec timeout = { 0, 1000000 }; // one millisecond
	return gpiod_line_event_wait(line, &timeout);
}

/**
 * @brief Read the last event from the GPIO line.
 * @param line GPIO line object.
 * @return event type if an event was read correctly (1 for rising edge, 2 for falling edge), -1 on error.
 * @note This function will block if no event was queued for this line.
 */
extern int ReadEventForLine(struct gpiod_line *line) 
{
	struct gpiod_line_event event;
	int response = gpiod_line_event_read(line, &event);
	if (response == 0) { // 0 returned means event read correctly, -1 on error
		return event.event_type;
	}
	return response;
}

/**
 * @brief Open a gpiochip by number.
 * @return GPIO chip pointer handle or NULL if an error occurred.
 */
extern struct gpiod_chip * OpenChipByNumber(int number)
{
    return gpiod_chip_open_by_number(number);
}
