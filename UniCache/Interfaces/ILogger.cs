using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UGIS.de.OfficeComponents.UniCacheLib
{

    /// <summary>
    /// Lightweight file logger 
    /// </summary>
    [ComVisible(true)]
    [Guid("A797AD07-35C4-4E47-8F67-E9F37C121AB5")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface ILogger
    {

        /// <summary>
        /// Gets the log filename.
        /// </summary>
        /// <value>
        /// The log filename.
        /// </value>
        [DispId(0x00)]
        String LogFilename { get; }

        /// <summary>
        /// Logs entering a code section and starts a clock to measure the runtime.
        /// </summary>
        /// <param name="sectionDescription">The section description.</param>
        /// <remarks>If <see cref="EnterSection(string)"/> has been called then <see cref="ExitSection()"/> (or <see cref="ExitSection()"/>) has to 
        /// be called onetime if the entered section is exited even in case of an exception within the entered section! 
        /// <para>Its a good practice to use <c>try</c>, <c>catch</c>- and <c>finally</c> to ensure this:</para>
        /// <para>
        /// Call <see cref="EnterSection(string)"/> immediately before a <c>try</c>-statement. Call <see cref="ExitSection()"/> or <see cref="ExitSection()"/> 
        /// immediately after the <c>catch</c>-statement, if the <c>catch</c>-code cannot throw an exception. If the <c>catch</c>-code might throw an exception 
        /// then call <see cref="ExitSection()"/> (or <see cref="ExitSection()"/>) within <c>finally</c>.
        /// </para>
        /// </remarks>        
        [DispId(0x01)]
        void EnterSection(String sectionDescription);


        /// <summary>
        /// Logs the exit of a entered section with runtime (in milliseconds).
        /// </summary>
        /// <remarks>Has to be called onetime if entered section is exited even in case of an exception within the entered section! 
        /// <para>Its a good practice to use <c>try</c>, <c>catch</c>- and <c>finally</c> to ensure this:</para>
        /// <para>
        /// Call <see cref="EnterSection(string)"/> (or <see cref="EnterSection(string)"/>) immediately before a <c>try</c>-statement. 
        /// Call <see cref="ExitSection()"/> immediately after the <c>catch</c>-statement, if the <c>catch</c>-code cannot throw an exception. 
        /// If the <c>catch</c>-code might throw an exception then call <see cref="ExitSection()"/> within <c>finally</c>.
        /// </para>
        /// </remarks>        
        [DispId(0x02)]
        void ExitSection();

        /// <summary>
        /// Appends the specified text to the text-buffer of this <see cref="Logger"/>-instance.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        /// <remarks>Call <see cref="Flush()"/> to append buffered text to the log file.</remarks>        
        [DispId(0x03)]
        void Write(String text);

        /// <summary>
        /// Appends the specified text and the default line terminator to the text-buffer of this <see cref="Logger"/>-instance.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        /// <remarks>Call <see cref="Flush()"/> to append buffered text to the log file.</remarks>
        [DispId(0x04)]
        void WriteLine(String text);

        /// <summary>
        /// Appends the specified text (which may contain curly-braces-placeholders) and a timestamp to the text-buffer of this <see cref="Logger" />-instance.
        /// </summary>
        /// <param name="text">The text to be written which may contain curly-braces-placeholders for <see cref="System.String.Format(string, object[])" />.</param>
        /// <param name="args">Content-array to fill the curly-braces-placeholders of <paramref name="text"/> (see <see cref="System.String.Format(string, object[])"/>).</param>
        /// <remarks>
        /// Call <see cref="Flush()" /> to append buffered text to the log file.
        /// </remarks>
        [DispId(0x05)]
        void WriteAppendTime(String text);

        /// <summary>
        /// Appends the specified text and a timestamp followed by the default line terminator to the text-buffer of this <see cref="Logger"/>-instance .
        /// </summary>
        /// <param name="text">The text to be written.</param>
        /// <remarks>Call <see cref="Flush()"/> to append buffered text to the log file.</remarks>
        [DispId(0x06)]
        void WriteLineAppendTime(String text);

        /// <summary>
        /// Appends the specified text and the description of the given <see cref="Exception" /> to the text-buffer of this <see cref="Logger" />-instance .
        /// </summary>
        /// <param name="text">The text to be written.</param>
        /// <param name="exceptionText">The text for an exception to be logged.</param>
        /// <remarks>
        /// After writing the exception to the text-buffer it will be flushed and written to the file.
        /// </remarks>
        [DispId(0x07)]
        void WriteException(String text, String exceptionText);

        /// <summary>
        /// Appends the default line terminator (<see cref="System.Text.StringBuilder.AppendLine()"/>) to the text-buffer of this <see cref="Logger"/>-instance .
        /// </summary>
        /// <remarks>Call <see cref="Flush()"/> to append buffered text to the log file.</remarks>        [DispId(0x08)]
        [DispId(0x08)]
        void WriteNewline();

        /// <summary>
        /// Appends a delimiter string (see <c>HTMLLOGDELIMITER</c>) to the text-buffer of this <see cref="Logger"/>-instance .
        /// </summary>
        /// <remarks>Call <see cref="Flush()"/> to append buffered text to the log file.</remarks>
        [DispId(0x09)]
        void WriteDelimiter();

        /// <summary>
        /// Flushes the log by appending the logged text to the log file.
        /// </summary>        [DispId(0x0A)]
        [DispId(0x0A)]
        void Flush();
    
    }

}
